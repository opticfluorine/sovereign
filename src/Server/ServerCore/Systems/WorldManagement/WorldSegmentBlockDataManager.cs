/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
/// Manages world segment summary block data for transmission to clients.
/// </summary>
public sealed class WorldSegmentBlockDataManager
{

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    private readonly BlockPositionEventFilter eventFilter;
    private readonly WorldSegmentBlockDataGenerator generator;

    /// <summary>
    /// Summary block data for loaded world segments via their producer tasks.
    /// A producer task is created when segment load is completed.
    /// Updates are made via continuations of the tasks.
    /// This allows the REST endpoint to await any segment data.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task<WorldSegmentBlockData>> dataProducers
        = new ConcurrentDictionary<GridPosition, Task<WorldSegmentBlockData>>();

    /// <summary>
    /// Deletion tasks. The presence of a deletion task in this map indicates
    /// that the deletion has been scheduled but is not yet complete.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, Task> deletionTasks
        = new ConcurrentDictionary<GridPosition, Task>();

    public WorldSegmentBlockDataManager(BlockPositionEventFilter eventFilter,
        WorldSegmentBlockDataGenerator generator)
    {
        this.eventFilter = eventFilter;
        this.generator = generator;
    }

    /// <summary>
    /// Gets summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data, or null if no summary block data is available.</returns>
    public Task<WorldSegmentBlockData> GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        return dataProducers.TryGetValue(segmentIndex, out var data) ? data : null;
    }

    /// <summary>
    /// Asynchronously adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void AddWorldSegment(GridPosition segmentIndex)
    {
        if (deletionTasks.TryGetValue(segmentIndex, out var deletionTask))
        {
            // Segment was rapidly unloaded and reloaded.
            // Schedule the reload for after the unload is complete.
            dataProducers[segmentIndex] = deletionTask.ContinueWith((task) => DoAddWorldSegment(segmentIndex));
        }
        else
        {
            // Segment has not yet been loaded or has been fully unloaded.
            // Start a new processing chain from scratch for this segment.
            dataProducers[segmentIndex] = Task.Factory.StartNew(() => DoAddWorldSegment(segmentIndex));
        }
    }

    /// <summary>
    /// Asynchronously updates a world segment in the data set.
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
        {
            dataProducers[segmentIndex] = currentTask.ContinueWith(
                (task) => DoUpdateWorldSegment(segmentIndex, task.Result));
        }
        else
        {
            Logger.ErrorFormat("Tried to update world segment data for {0} before it was added.", segmentIndex);
        }
    }

    /// <summary>
    /// Asynchronously removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void RemoveWorldSegment(GridPosition segmentIndex)
    {
        // If the world segment is already being removed, don't attempt to remove it again.
        if (deletionTasks.ContainsKey(segmentIndex))
        {
            return;
        }

        // Immediately remove the segment from the data set, then schedule it for disposal.
        if (dataProducers.TryRemove(segmentIndex, out var currentTask))
        {
            deletionTasks[segmentIndex] = currentTask.ContinueWith(
                (task) => DoRemoveWorldSegment(segmentIndex, currentTask.Result));
        }
        else
        {
            Logger.ErrorFormat("Tried to remove world segemnt data for {0} before it was added.", segmentIndex);
        }
    }

    /// <summary>
    /// Blocking call that adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private WorldSegmentBlockData DoAddWorldSegment(GridPosition segmentIndex)
    {
        try
        {
            Logger.DebugFormat("Adding summary block data for world segment {0}.", segmentIndex);
            return generator.Create(segmentIndex);
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error adding summary block data for world segment {0}.", segmentIndex);
            return null;
        }
    }

    /// <summary>
    /// Blockign call that updates a world segment in the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="data">Existing world segment data.</param>
    /// <returns>Updated world segment data.</returns>
    private WorldSegmentBlockData DoUpdateWorldSegment(GridPosition segmentIndex, WorldSegmentBlockData data)
    {
        // TODO
        return data;
    }

    /// <summary>
    /// Blockign call that removes a world segment from the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="data">Existing world segment data.</param>
    private void DoRemoveWorldSegment(GridPosition segmentIndex, WorldSegmentBlockData data)
    {
        try
        {
            // TODO Return lists to object pool for future reuse.
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error removing summary block data for world segment {0}.", segmentIndex);
        }
        finally
        {
            // Clear deletion task.
            deletionTasks.TryRemove(segmentIndex, out var unused);
        }
    }

}
