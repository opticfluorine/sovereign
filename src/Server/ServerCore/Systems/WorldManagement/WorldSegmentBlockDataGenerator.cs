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
        var blocks = new List<PositionedEntity>();
        using (var lockHandle = indexer.AcquireLock())
        {
            indexer.GetEntitiesInRange(lockHandle, range.Item1, range.Item2, blocks);
        }

        // Retrieve material data, group into depth planes.
        var depthPlanes = new List<BlockData>[config.SegmentLength];
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            depthPlanes[i] = new List<BlockData>();
        }
        GroupByDepth(blocks, depthPlanes, segmentIndex);

        // Initialize a new block summary data object.
        WorldSegmentBlockData data = GetEmptyBlockData();

        // Process each depth plane separately.
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            if (TryConvertDepthPlane(depthPlanes[i], ref data.DefaultMaterialsPerPlane[i],
                out var converted))
            {
                // At least one non-default block in plane.
                data.DataPlanes.Add(converted);
            }
        }

        // TODO
        return null;
    }

    public void Free(WorldSegmentBlockData data)
    {
        // TODO
    }

    /// <summary>
    /// Gets an empty block data object for use.
    /// </summary>
    /// <returns>Empty block data object.</returns>
    private WorldSegmentBlockData GetEmptyBlockData()
    {
        var data = new WorldSegmentBlockData();
        data.DefaultMaterialsPerPlane = new BlockMaterialData[config.SegmentLength];
        data.DataPlanes = new List<WorldSegmentBlockDataPlane>();

        return data;
    }

    /// <summary>
    /// Groups a list of block entities into depth planes while also retrieving material data.
    /// </summary>
    /// <param name="blocks">Blocks in the world segment.</param>
    /// <param name="depthPlanes">Depth planes to populate, indexed by z offset from segment origin.</param>
    /// <param name="segmentIndex">World segment index.</param>
    private void GroupByDepth(List<PositionedEntity> blocks, List<BlockData>[] depthPlanes, GridPosition segmentIndex)
    {
        var baseDepth = (int)resolver.GetRangeForWorldSegment(segmentIndex).Item1.Z;
        foreach (var block in blocks)
        {
            var offset = (int)block.Position.Z - baseDepth;
            var material = materials[block.EntityId];
            var modifier = modifiers[block.EntityId];

            var blockData = new BlockData
            {
                Material = material,
                MaterialModifier = modifier,
                Position = new GridPosition(block.Position)
            };

            depthPlanes[offset].Add(blockData);
        }
    }

    /// <summary>
    /// Converts a depth plane. The default block is always updated. If any non-default blocks
    /// are present in the plane, a converted data plane is passed back.
    /// </summary>
    /// <param name="blocks">Input block data for this depth plane.</param>
    /// <param name="defaultMaterial">Default material for the plane.</param>
    /// <param name="converted">Converted plane. Undefined if the method returns false.</param>
    /// <returns>true if any non-default blocks were converted; false otherwise.</returns>
    private bool TryConvertDepthPlane(List<BlockData> blocks, ref BlockMaterialData defaultMaterial,
        out WorldSegmentBlockDataPlane converted)
    {
        converted = new WorldSegmentBlockDataPlane();

        return false;
    }


}
