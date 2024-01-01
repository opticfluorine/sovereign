/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MessagePack;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Manages world segment summary block data for transmission to clients.
/// </summary>
public sealed class WorldSegmentBlockDataManager
{
    /// <summary>
    ///     Summary block data for loaded world segments via their producer tasks.
    ///     A producer task is created when segment load is completed.
    ///     Updates are made via continuations of the tasks.
    ///     This allows the REST endpoint to await any segment data.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task<byte[]?>> dataProducers = new();

    /// <summary>
    ///     Deletion tasks. The presence of a deletion task in this map indicates
    ///     that the deletion has been scheduled but is not yet complete.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task> deletionTasks = new();

    private readonly BlockPositionEventFilter eventFilter;
    private readonly WorldSegmentBlockDataGenerator generator;

    public WorldSegmentBlockDataManager(BlockPositionEventFilter eventFilter,
        WorldSegmentBlockDataGenerator generator)
    {
        this.eventFilter = eventFilter;
        this.generator = generator;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Gets summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data, or null if no summary block data is available.</returns>
    public Task<byte[]?>? GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        return dataProducers.TryGetValue(segmentIndex, out var data) ? data : null;
    }

    /// <summary>
    ///     Asynchronously adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void AddWorldSegment(GridPosition segmentIndex)
    {
        if (deletionTasks.TryGetValue(segmentIndex, out var deletionTask))
            // Segment was rapidly unloaded and reloaded.
            // Schedule the reload for after the unload is complete.
            dataProducers[segmentIndex] = deletionTask.ContinueWith(task => DoAddWorldSegment(segmentIndex));
        else
            // Segment has not yet been loaded or has been fully unloaded.
            // Start a new processing chain from scratch for this segment.
            dataProducers[segmentIndex] = Task.Factory.StartNew(() => DoAddWorldSegment(segmentIndex));
    }

    /// <summary>
    ///     Asynchronously updates a world segment in the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void UpdateWorldSegment(GridPosition segmentIndex)
    {
        if (deletionTasks.ContainsKey(segmentIndex))
        {
            // Segment already being deleted, do not update.
            Logger.WarnFormat("Tried to update world segment data for {0} during deletion.", segmentIndex);
            return;
        }

        if (dataProducers.TryGetValue(segmentIndex, out var currentTask))
            // Just redo the creation instead of trying to do an incremental update.
            // We can come back here and optimize later if this causes too big of a
            // performance hit.
            dataProducers[segmentIndex] = currentTask.ContinueWith(
                task => DoAddWorldSegment(segmentIndex));
    }

    /// <summary>
    ///     Asynchronously removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void RemoveWorldSegment(GridPosition segmentIndex)
    {
        // If the world segment is already being removed, don't attempt to remove it again.
        if (deletionTasks.ContainsKey(segmentIndex)) return;

        // Immediately remove the segment from the data set, then schedule it for disposal.
        if (dataProducers.TryRemove(segmentIndex, out var currentTask))
            deletionTasks[segmentIndex] = currentTask.ContinueWith(
                task => DoRemoveWorldSegment(segmentIndex));
        else
            Logger.ErrorFormat("Tried to remove world segemnt data for {0} before it was added.", segmentIndex);
    }

    /// <summary>
    ///     Blocking call that adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private byte[]? DoAddWorldSegment(GridPosition segmentIndex)
    {
        try
        {
            Logger.DebugFormat("Adding summary block data for world segment {0}.", segmentIndex);
            var blockData = generator.Create(segmentIndex);
            return MessagePackSerializer.Serialize(blockData, MessageConfig.CompressedUntrustedMessagePackOptions);
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error adding summary block data for world segment {0}.", segmentIndex);
            return null;
        }
    }

    /// <summary>
    ///     Blocking call that removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void DoRemoveWorldSegment(GridPosition segmentIndex)
    {
        // Clear deletion task.
        deletionTasks.TryRemove(segmentIndex, out var unused);
    }
}