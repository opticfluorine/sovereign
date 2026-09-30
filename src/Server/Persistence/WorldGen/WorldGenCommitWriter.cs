// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen.Output;

namespace Sovereign.Persistence.WorldGen;

/// <summary>
///     Default IWorldGenCommitWriter. Writes staged plan segment blobs and decoration
///     entities to the database in batched, idempotent transactions.
/// </summary>
public sealed class WorldGenCommitWriter : IWorldGenCommitWriter
{
    private readonly PersistenceProviderManager providerManager;
    private readonly EntityAssigner entityAssigner;

    public WorldGenCommitWriter(PersistenceProviderManager providerManager,
        EntityAssigner entityAssigner)
    {
        this.providerManager = providerManager;
        this.entityAssigner = entityAssigner;
    }

    /// <summary>
    ///     Writes the staged plan to the database.
    /// </summary>
    /// <param name="request">Commit parameters.</param>
    /// <returns>Counts of the performed writes.</returns>
    public WorldGenCommitStats Execute(WorldGenCommitRequest request)
    {
        var clock = Stopwatch.StartNew();
        var segmentIndices = ReadSegmentIndices(request.StagingDirectory);
        var decorations = ReadDecorations(request.StagingDirectory);
        var segmentsPerBatch = Math.Max(1, request.BatchSize);
        var decorationsPerBatch = Math.Max(1, request.BatchSize);
        var totalBatches = CeilDiv(segmentIndices.Count, segmentsPerBatch)
                           + CeilDiv(decorations.Count, decorationsPerBatch)
                           + (request.ReplaceWorld is null ? 0 : 1);
        var completedBatches = 0;
        var decorationsDeleted = 0;

        if (request.ReplaceWorld is { } replaced)
        {
            decorationsDeleted = DeleteReplacedWorld(request.StagingDirectory, replaced);
            ++completedBatches;
            request.Progress?.Invoke(completedBatches, totalBatches);
            request.AfterBatch?.Invoke(completedBatches);
        }

        foreach (var batch in BatchesOf(segmentIndices, segmentsPerBatch))
        {
            WriteSegmentBatch(request.StagingDirectory, batch);
            ++completedBatches;
            request.Progress?.Invoke(completedBatches, totalBatches);
            request.AfterBatch?.Invoke(completedBatches);
        }

        var decorationsCreated = 0;
        foreach (var batch in BatchesOf(decorations, decorationsPerBatch))
        {
            decorationsCreated += WriteDecorationBatch(batch);
            ++completedBatches;
            request.Progress?.Invoke(completedBatches, totalBatches);
            request.AfterBatch?.Invoke(completedBatches);
        }

        clock.Stop();
        return new WorldGenCommitStats
        {
            SegmentsWritten = segmentIndices.Count,
            DecorationsCreated = decorationsCreated,
            DecorationsDeleted = decorationsDeleted,
            WallMs = clock.ElapsedMilliseconds
        };
    }

    /// <summary>
    ///     Reads the world segment indices of every staged segment blob, in deterministic
    ///     scan order.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <returns>Sorted segment indices.</returns>
    private static List<GridPosition> ReadSegmentIndices(string stagingDirectory)
    {
        var directory = Path.Combine(stagingDirectory, "segments");
        var indices = new List<GridPosition>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.bin"))
        {
            var parts = Path.GetFileNameWithoutExtension(path).Split('_');
            if (parts.Length != 3
                || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
                || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z))
            {
                throw new InvalidOperationException(
                    $"Staged segment file name \"{path}\" is invalid.");
            }

            indices.Add(new GridPosition { X = x, Y = y, Z = z });
        }

        indices.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X)
            : a.Y != b.Y ? a.Y.CompareTo(b.Y)
            : a.Z.CompareTo(b.Z));
        return indices;
    }

    /// <summary>
    ///     Reads the staged decoration rows.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <returns>Staged decorations in placement order.</returns>
    private static List<StagedDecoration> ReadDecorations(string stagingDirectory)
    {
        var path = Path.Combine(stagingDirectory, "decorations.bin");
        if (!File.Exists(path)) return new List<StagedDecoration>();

        return MessageConfig.DeserializeMsgPack<List<StagedDecoration>>(File.ReadAllBytes(path))
               ?? new List<StagedDecoration>();
    }

    /// <summary>
    ///     Deletes the segment rows and decoration entities of a replaced world within its
    ///     registered footprint, including the segment Z range of the staged plan so that
    ///     an old world with a different vertical extent leaves no stale rows.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory of the staged plan.</param>
    /// <param name="replaced">Registered world being replaced.</param>
    /// <returns>Number of decoration entities deleted.</returns>
    private int DeleteReplacedWorld(string stagingDirectory, WorldGenRegistryEntry replaced)
    {
        var provider = providerManager.PersistenceProvider;
        var (minSegmentZ, maxSegmentZ) = StagedPlanZRange(stagingDirectory, replaced);
        provider.TransactionLock.Acquire();
        try
        {
            using var transaction = provider.Connection.BeginTransaction();
            provider.DeleteWorldSegmentBlockDataInRangeQuery.DeleteInRange(
                replaced.OriginX >> 5, ((replaced.OriginX + replaced.Width - 1) >> 5) + 1,
                replaced.OriginY >> 5, ((replaced.OriginY + replaced.Height - 1) >> 5) + 1,
                Math.Min(replaced.MinSegmentZ, minSegmentZ), Math.Max(replaced.MaxSegmentZ, maxSegmentZ) + 1,
                transaction);
            var deleted = provider.DeleteWorldGenDecorationsInRangeQuery.DeleteInRange(
                replaced.OriginX, replaced.OriginX + replaced.Width,
                replaced.OriginY, replaced.OriginY + replaced.Height,
                replaced.MinSegmentZ * 32, (replaced.MaxSegmentZ + 1) * 32,
                transaction);
            transaction.Commit();
            return deleted;
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Writes one batch of staged segment blobs in a single transaction.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <param name="batch">Segment indices of the batch.</param>
    private void WriteSegmentBatch(string stagingDirectory, IReadOnlyList<GridPosition> batch)
    {
        var provider = providerManager.PersistenceProvider;
        provider.TransactionLock.Acquire();
        try
        {
            using var transaction = provider.Connection.BeginTransaction();
            foreach (var segmentIndex in batch)
            {
                var path = Path.Combine(stagingDirectory, "segments",
                    $"{segmentIndex.X}_{segmentIndex.Y}_{segmentIndex.Z}.bin");
                provider.SetWorldSegmentBlockDataQuery.SetWorldSegmentBlockData(
                    segmentIndex, File.ReadAllBytes(path), transaction);
            }

            transaction.Commit();
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Writes one batch of staged decorations as bulk entity rows, allocating the
    ///     entity ID block from the database and advancing the in-memory entity assigner
    ///     past the block.
    /// </summary>
    /// <param name="batch">Staged decorations of the batch.</param>
    /// <returns>Number of decoration entities created.</returns>
    private int WriteDecorationBatch(IReadOnlyList<StagedDecoration> batch)
    {
        if (batch.Count == 0) return 0;

        var provider = providerManager.PersistenceProvider;
        provider.TransactionLock.Acquire();
        try
        {
            var firstId = provider.NextPersistedIdQuery.GetNextPersistedEntityId();
            var rows = new List<BulkEntityRow>(batch.Count);
            for (var i = 0; i < batch.Count; ++i)
            {
                var decoration = batch[i];
                rows.Add(new BulkEntityRow
                {
                    EntityId = firstId + (ulong)i,
                    TemplateEntityId = decoration.TemplateEntityId,
                    X = decoration.X,
                    Y = decoration.Y,
                    Z = decoration.Z,
                    EntityType = 0
                });
            }

            using (var transaction = provider.Connection.BeginTransaction())
            {
                provider.BulkAddEntitiesQuery.AddEntities(rows, transaction);
                transaction.Commit();
            }

            // The assigner hands out IDs above its stored value, so park it at the last
            // ID of the allocated block.
            var lastId = firstId + (ulong)batch.Count - 1;
            if (entityAssigner.NextEntityId < lastId)
            {
                entityAssigner.NextEntityId = lastId;
            }

            return rows.Count;
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Computes the segment Z range spanned by the staged plan and the replaced world.
    ///     The staged manifest is not parsed; the replaced world's Z range is extended to
    ///     cover the full plausible vertical extent from the bedrock Z up.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <param name="replaced">Registered world being replaced.</param>
    /// <returns>Segment Z range to clear.</returns>
    private static (int MinSegmentZ, int MaxSegmentZ) StagedPlanZRange(string stagingDirectory,
        WorldGenRegistryEntry replaced)
    {
        // The staged plan's own Z range is derived from its manifest when present;
        // otherwise the replaced world's range suffices because replace requires the
        // staged footprint to contain the registered one.
        var manifestPath = Path.Combine(stagingDirectory, "plan.json");
        if (File.Exists(manifestPath))
        {
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(manifestPath));
                var root = document.RootElement;
                return (root.GetProperty("minSegmentZ").GetInt32(),
                    root.GetProperty("maxSegmentZ").GetInt32());
            }
            catch (Exception)
            {
                // Fall through to the replaced world's Z range.
            }
        }

        return (replaced.MinSegmentZ, replaced.MaxSegmentZ);
    }

    /// <summary>
    ///     Splits a list into consecutive batches of the given size.
    /// </summary>
    /// <param name="items">Items to split.</param>
    /// <param name="size">Batch size.</param>
    /// <typeparam name="T">Item type.</typeparam>
    /// <returns>Batches.</returns>
    private static IEnumerable<IReadOnlyList<T>> BatchesOf<T>(IReadOnlyList<T> items, int size)
    {
        for (var offset = 0; offset < items.Count; offset += size)
        {
            yield return items.Skip(offset).Take(size).ToList();
        }
    }

    /// <summary>
    ///     Computes the ceiling of an integer division.
    /// </summary>
    /// <param name="value">Numerator.</param>
    /// <param name="denominator">Denominator.</param>
    /// <returns>Ceiling of the quotient.</returns>
    private static int CeilDiv(int value, int denominator)
    {
        return (value + denominator - 1) / denominator;
    }
}
