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

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Parameters of one commit write.
/// </summary>
public sealed class WorldGenCommitRequest
{
    /// <summary>
    ///     Staging directory of the plan to write.
    /// </summary>
    public required string StagingDirectory { get; init; }

    /// <summary>
    ///     Number of segments or decorations per transaction.
    /// </summary>
    public required int BatchSize { get; init; }

    /// <summary>
    ///     Registered world to delete before writing, or null.
    /// </summary>
    public WorldGenRegistryEntry? ReplaceWorld { get; init; }

    /// <summary>
    ///     Optional callback invoked after each committed batch with the completed and
    ///     total batch counts.
    /// </summary>
    public Action<int, int>? Progress { get; init; }

    /// <summary>
    ///     Optional test hook invoked after each committed batch with the one-based batch
    ///     count; a throwing hook simulates an interruption.
    /// </summary>
    public Action<int>? AfterBatch { get; init; }
}

/// <summary>
///     Counts of the database writes performed by one commit run.
/// </summary>
public sealed class WorldGenCommitStats
{
    /// <summary>
    ///     Number of segment blobs written.
    /// </summary>
    public required int SegmentsWritten { get; init; }

    /// <summary>
    ///     Number of decoration entities created.
    /// </summary>
    public required int DecorationsCreated { get; init; }

    /// <summary>
    ///     Number of decoration entities deleted by a replace.
    /// </summary>
    public required int DecorationsDeleted { get; init; }

    /// <summary>
    ///     Wall time of the commit in milliseconds.
    /// </summary>
    public required long WallMs { get; init; }
}

/// <summary>
///     Writes a staged world generation plan to the database in batched, idempotent
///     transactions: each batch re-deletes and re-inserts its rows, so an interrupted
///     commit can simply be re-run.
/// </summary>
public interface IWorldGenCommitWriter
{
    /// <summary>
    ///     Writes the staged plan to the database.
    /// </summary>
    /// <param name="request">Commit parameters.</param>
    /// <returns>Counts of the performed writes.</returns>
    WorldGenCommitStats Execute(WorldGenCommitRequest request);
}
