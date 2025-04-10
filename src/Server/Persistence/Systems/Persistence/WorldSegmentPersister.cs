// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineUtil.Collections;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for persisting modified world segment block data.
/// </summary>
public class WorldSegmentPersister
{
    /// <summary>
    ///     Size in bytes of buffer to hold the loaded world segment block data.
    /// </summary>
    private const int BufferSize = 1048576;

    private readonly ObjectPool<PersistenceBuffer> bufferPool = new();
    private readonly CoreWorldManagementController coreWorldManagementController;
    private readonly IEventSender eventSender;

    private readonly WorldSegmentBlockDataLoader loader;
    private readonly ILogger<WorldSegmentPersister> logger;

    private readonly WorldManagementServices worldManagementServices;

    public WorldSegmentPersister(WorldManagementServices worldManagementServices, WorldSegmentBlockDataLoader loader,
        IEventSender eventSender, CoreWorldManagementController coreWorldManagementController,
        ILogger<WorldSegmentPersister> logger)
    {
        this.worldManagementServices = worldManagementServices;
        this.loader = loader;
        this.eventSender = eventSender;
        this.coreWorldManagementController = coreWorldManagementController;
        this.logger = logger;
    }

    /// <summary>
    ///     Loads the world segment block data for the given world segment from the database.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <remarks>
    ///     This method is not thread-safe. It should only be called from the executor thread of
    ///     the PersistenceSystem.
    /// </remarks>
    public void LoadWorldSegmentBlockData(IPersistenceProvider provider, GridPosition segmentIndex)
    {
        logger.LogDebug("Get block data for {Index} from DB.", segmentIndex);
        var blockDataBuffer = bufferPool.TakeObject();
        WorldSegmentBlockData? blockData;
        try
        {
            if (!provider.GetWorldSegmentBlockDataQuery.TryGetWorldSegmentBlockData(segmentIndex,
                    blockDataBuffer.Buffer))
            {
                // This is a new segment with no persisted data, so declare it loaded.
                coreWorldManagementController.AnnounceWorldSegmentLoaded(eventSender, segmentIndex);
                return;
            }

            blockData = MessageConfig.DeserializeMsgPack<WorldSegmentBlockData>(blockDataBuffer.Buffer)
                        ?? throw new Exception(
                            $"Persisted block data for world segment {segmentIndex} is invalid.");
        }
        finally
        {
            bufferPool.ReturnObject(blockDataBuffer);
        }

        loader.Load(segmentIndex, blockData);
    }

    /// <summary>
    ///     Synchronizes modified world segment block data to the database.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    public void SynchronizeWorldSegments(IPersistenceProvider provider, IDbTransaction transaction)
    {
        var segmentIndices = worldManagementServices.GetAndClearSegmentsToPersist();
        var dataTasks = segmentIndices
            .Select(index => Tuple.Create(index, worldManagementServices.GetWorldSegmentBlockData(index)))
            .ToList();
        var tasks = dataTasks.Select(task => task.Item2).ToArray();

        Task.WaitAll(tasks);

        foreach (var dataTask in dataTasks)
        {
            var (segmentIndex, task) = dataTask;
            if (task.IsFaulted) throw task.Exception ?? new Exception("Data task faulted.");
            SyncSingleData(segmentIndex, task.Result.Item1, provider, transaction);
        }
    }

    /// <summary>
    ///     Synchronizes the block data for a single world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="data">Data.</param>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Database transaction.</param>
    private void SyncSingleData(GridPosition segmentIndex, WorldSegmentBlockData data, IPersistenceProvider provider,
        IDbTransaction transaction)
    {
        var serializedData = MessageConfig.SerializeMsgPack(data);
        provider.SetWorldSegmentBlockDataQuery.SetWorldSegmentBlockData(segmentIndex, serializedData, transaction);
    }

    /// <summary>
    ///     Persistence buffer type.
    /// </summary>
    private class PersistenceBuffer
    {
        public readonly byte[] Buffer = new byte[BufferSize];
    }
}