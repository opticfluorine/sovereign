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
    /// Summary block data for loaded world segments.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, WorldSegmentBlockData> segmentBlockData
        = new ConcurrentDictionary<GridPosition, WorldSegmentBlockData>();

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
    public WorldSegmentBlockData GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        return segmentBlockData.TryGetValue(segmentIndex, out var data) ? data : null;
    }

    /// <summary>
    /// Asynchronously adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void AddWorldSegment(GridPosition segmentIndex)
    {
        Task.Factory.StartNew(() => DoAddWorldSegment(segmentIndex));
    }

    /// <summary>
    /// Blocking call that adds a world segment to the data set.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void DoAddWorldSegment(GridPosition segmentIndex)
    {
        try
        {
            Logger.DebugFormat("Adding summary block data for world segment {0}.", segmentIndex);
            segmentBlockData[segmentIndex] = generator.Create(segmentIndex);
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Error adding summary block data for world segment {0}.", segmentIndex);
        }
    }

}
