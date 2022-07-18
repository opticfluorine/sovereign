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
using System.Collections.Generic;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineUtil.Collections;
using Sovereign.WorldManagement.Configuration;
using Sovereign.WorldManagement.WorldSegments;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
/// Responsible for creating and updating summary world segment block data structures.
/// </summary>
public sealed class WorldSegmentBlockDataGenerator
{
    private readonly WorldSegmentResolver resolver;
    private readonly BlockPositionIndexer indexer;
    private readonly MaterialComponentCollection materials;
    private readonly MaterialModifierComponentCollection modifiers;
    private readonly IWorldManagementConfiguration config;

    /// <summary>
    /// Internal block data type.
    /// </summary>
    private struct BlockData
    {
        /// <summary>
        /// Block position.
        /// </summary>
        public GridPosition Position;

        /// <summary>
        /// Material ID.
        /// </summary>
        public int Material;

        /// <summary>
        /// Material modifier ID.
        /// </summary>
        public int MaterialModifier;
    }

    /// <summary>
    /// Object pool for block buffers.
    /// </summary>
    private readonly ObjectPool<List<PositionedEntity>> blockListPool = new ObjectPool<List<PositionedEntity>>();

    /// <summary>
    /// Object pool for nested lists breaking down a segment into depth planes.
    /// </summary>
    private readonly ObjectPool<List<List<BlockData>>> planeListPool = new ObjectPool<List<List<BlockData>>>();

    /// <summary>
    /// Object pool for world segment block data objects.
    /// </summary>
    private readonly ObjectPool<WorldSegmentBlockData> dataPool = new ObjectPool<WorldSegmentBlockData>();

    /// <summary>
    /// Object pool for converted data planes.
    /// </summary>
    private readonly ObjectPool<List<WorldSegmentBlockDataPlane>> convertedPlaneListPool
        = new ObjectPool<List<WorldSegmentBlockDataPlane>>();

    /// <summary>
    /// Object pool for converted data lines.
    /// </summary>
    private readonly ObjectPool<List<WorldSegmentBlockDataLine>> convertedLineListPool
        = new ObjectPool<List<WorldSegmentBlockDataLine>>();

    public WorldSegmentBlockDataGenerator(WorldSegmentResolver resolver, BlockPositionIndexer indexer,
        MaterialComponentCollection materials, MaterialModifierComponentCollection modifiers,
        IWorldManagementConfiguration config)
    {
        this.resolver = resolver;
        this.indexer = indexer;
        this.materials = materials;
        this.modifiers = modifiers;
        this.config = config;
    }

    /// <summary>
    /// Creates summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data.</returns>
    public WorldSegmentBlockData Create(GridPosition segmentIndex)
    {
        // Retrieve the latest data for the requested world segment.
        var range = resolver.GetRangeForWorldSegment(segmentIndex);
        var blocks = blockListPool.TakeObject();
        using (var lockHandle = indexer.AcquireLock())
        {
            indexer.GetEntitiesInRange(lockHandle, range.Item1, range.Item2, blocks);
        }

        // Retrieve material data, group into depth planes.
        var depthPlanes = planeListPool.TakeObject();
        if (depthPlanes.Count < config.SegmentLength)
        {
            // This appears to be a new list, allocate depth planes.
            InitializeDepthPlanes(depthPlanes);
        }
        GroupByDepth(blocks, depthPlanes, segmentIndex);

        // Initialize a new block summary data object.
        WorldSegmentBlockData data = GetEmptyBlockData();

        // Process each depth plane separately.

        // Clean up.
        foreach (var plane in depthPlanes) plane.Clear();
        blocks.Clear();
        planeListPool.ReturnObject(depthPlanes);
        blockListPool.ReturnObject(blocks);

        // TODO
        return null;
    }


    /// <summary>
    /// Cleans up a block data object produced by this generator, returning any
    /// pooled objects to their respective object pools. The object should be
    /// considered invalid following this call.
    /// </summary>
    /// <param name="data">Data to be freed.</param>
    public void Free(WorldSegmentBlockData data)
    {
        // TODO Clean up internal structures.

        dataPool.ReturnObject(data);
    }

    /// <summary>
    /// Initializes a new list of depth planes.
    /// </summary>
    /// <param name="depthPlanes">Depth planes list to initialize.</param>
    private void InitializeDepthPlanes(List<List<BlockData>> depthPlanes)
    {
        depthPlanes.Clear();
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            depthPlanes.Add(new List<BlockData>());
        }
    }

    /// <summary>
    /// Gets an empty block data object for use.
    /// </summary>
    /// <returns>Empty block data object.</returns>
    private WorldSegmentBlockData GetEmptyBlockData()
    {
        var data = dataPool.TakeObject();

        if (data.DefaultMaterialsPerPlane == null)
        {
            // Take this as indication that the structure hasn't been initialized yet.
            data.DefaultMaterialsPerPlane = new BlockMaterialData[config.SegmentLength];
            data.DataPlanes = new List<WorldSegmentBlockDataPlane>();
        }

        return data;
    }

    /// <summary>
    /// Cleans up a block data object, returning objects to the pools as needed.
    /// The data object itself is also returned to its pool; the reference should be
    /// considered invalid after this method returns.
    /// </summary>
    /// <param name="data">Block data object to clean up.</param>
    private void CleanupBlockData(WorldSegmentBlockData data)
    {
        foreach (var plane in data.DataPlanes)
        {
            foreach (var line in plane.Lines)
            {
            }
            plane.Lines.Clear();
            convertedLineListPool.ReturnObject(plane.Lines);
        }
        data.DataPlanes.Clear();
        dataPool.ReturnObject(data);
    }

    /// <summary>
    /// Groups a list of block entities into depth planes while also retrieving material data.
    /// </summary>
    /// <param name="blocks">Blocks in the world segment.</param>
    /// <param name="depthPlanes">Depth planes to populate, indexed by z offset from segment origin.</param>
    /// <param name="segmentIndex">World segment index.</param>
    private void GroupByDepth(List<PositionedEntity> blocks, List<List<BlockData>> depthPlanes, GridPosition segmentIndex)
    {

    }

}
