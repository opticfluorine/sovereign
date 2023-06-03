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
using Sovereign.EngineCore.World.Materials;
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
    private class BlockData
    {
        /// <summary>
        /// Block position.
        /// </summary>
        public GridPosition Position;

        /// <summary>
        /// Block type.
        /// </summary>
        public BlockMaterialData BlockType;
    }

    /// <summary>
    /// Internal struct used for grouping blocks into depth planes.
    /// </summary>
    private class DepthPlane
    {
        /// <summary>
        /// Blocks in the depth plane.
        /// </summary>
        public List<BlockData> Blocks = new List<BlockData>();

        /// <summary>
        /// Default block type for this depth plane.
        /// </summary>
        public BlockMaterialData DefaultBlockType { get; private set; }
            = new BlockMaterialData() { MaterialId = Material.Air, ModifierId = 0 };

        /// <summary>
        /// Z offset of this depth plane.
        /// </summary>
        public int OffsetZ { get; private set; }

        /// <summary>
        /// Running count of material types in this depth plane.
        /// </summary>
        private Dictionary<BlockMaterialData, int> MaterialCounts
            = new Dictionary<BlockMaterialData, int>();

        /// <summary>
        /// Cached count of most frequent material type.
        /// </summary>
        private int LeadingMaterialCount = 0;

        /// <summary>
        /// Creates an empty depth plane.
        /// </summary>
        /// <param name="offsetZ">Z offset of this depth plane.</param>
        public DepthPlane(int offsetZ)
        {
            OffsetZ = offsetZ;
        }

        /// <summary>
        /// Adds a block to the depth plane.
        /// </summary>
        /// <param name="block">Block.</param>
        public void Add(BlockData block)
        {
            Blocks.Add(block);

            // Update material counts.
            int count;
            if (MaterialCounts.TryGetValue(block.BlockType, out count))
            {
                count++;
            }
            else
            {
                count = 1;
            }
            MaterialCounts[block.BlockType] = count;

            // Update default block if needed.
            if (count > LeadingMaterialCount)
            {
                LeadingMaterialCount = count;
                DefaultBlockType = block.BlockType;
            }
        }

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
        var basePoint = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var depthPlanes = new DepthPlane[config.SegmentLength];
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            depthPlanes[i] = new DepthPlane(i);
        }
        GroupByDepth(blocks, depthPlanes, (int)basePoint.Z);

        // Initialize a new block summary data object.
        WorldSegmentBlockData data = GetEmptyBlockData();

        // Process each depth plane separately.
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            // Update defaults.
            data.DefaultMaterialsPerPlane[i] = depthPlanes[i].DefaultBlockType;

            // Add the layer if it contains non-default blocks.
            if (TryConvertDepthPlane(depthPlanes[i],
                (int)basePoint.X, (int)basePoint.Y, (int)basePoint.Z,
                out var converted))
            {
                // At least one non-default block in plane.
                data.DataPlanes.Add(converted);
            }
        }

        return data;
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
    /// This method also identifies the most frequent material appearing in each plane.
    /// </summary>
    /// <param name="blocks">Blocks in the world segment.</param>
    /// <param name="depthPlanes">Depth planes to populate, indexed by z offset from segment origin.</param>
    /// <param name="baseZ">Z coordinate of segment base point.</param>
    private void GroupByDepth(List<PositionedEntity> blocks, DepthPlane[] depthPlanes, int baseZ)
    {
        foreach (var block in blocks)
        {
            var offset = (int)block.Position.Z - baseZ;
            var material = materials[block.EntityId];
            var modifier = modifiers[block.EntityId];

            var blockData = new BlockData
            {
                BlockType = new BlockMaterialData() { MaterialId = material, ModifierId = modifier },
                Position = new GridPosition(block.Position)
            };

            depthPlanes[offset].Add(blockData);
        }
    }

    /// <summary>
    /// Converts a depth plane. If any non-default blocks are present in the
    /// depth plane, a data plane is passed back.
    /// </summary>
    /// <param name="depthPlane">Depth plane to convert.</param>
    /// <param name="baseX">Base X position of the data block.</param>
    /// <param name="baseY">Base Y position of the data block.</param>
    /// <param name="baseZ">Base Z position of the data block.</param>
    /// <param name="converted">Converted plane, only if method returns true.</param>
    /// <returns>true if any non-default blocks were converted; false otherwise.</returns>
    private bool TryConvertDepthPlane(DepthPlane depthPlane, 
        int baseX, int baseY, int baseZ,
        out WorldSegmentBlockDataPlane converted)
    {
        // Allocate all possible data lines.
        var lines = new WorldSegmentBlockDataLine[config.SegmentLength];
        for (int i = 0; i < lines.Length; ++i)
        {
            lines[i].OffsetY = (byte)i;
            lines[i].BlockData = new List<LinePositionedBlockData>();
        }

        // Filter to exclude default blocks.
        var keptBlocks = new List<BlockData>();
        foreach (var block in depthPlane.Blocks)
        {
            if (block.BlockType != depthPlane.DefaultBlockType)
            {
                keptBlocks.Add(block);
            }
        }

        // As a special case, if the default block is not air, the air blocks
        // must be created and added to the kept set.
        if (depthPlane.DefaultBlockType.MaterialId != Material.Air)
        {
            AddAirBlocks(keptBlocks, depthPlane, baseX, baseY, baseZ);
        }

        // Group blocks into data lines.
        foreach (var block in keptBlocks)
        {
            // Convert block.
            var positionedBlock = new LinePositionedBlockData()
            {
                OffsetX = (byte)(block.Position.X - baseX),
                MaterialData = block.BlockType
            };

            var offsetY = block.Position.Y - baseY;
            lines[offsetY].BlockData.Add(positionedBlock);
        }

        // Retain only the non-empty data lines in the converted plane.
        converted = new WorldSegmentBlockDataPlane();
        converted.OffsetZ = (byte)depthPlane.OffsetZ;
        var hasLines = false;
        foreach (var line in lines)
        {
            if (line.BlockData.Count > 0)
            {
                converted.Lines.Add(line);
                hasLines = true;
            }
        }

        return hasLines;
    }

    /// <summary>
    /// Generates explicit air blocks in the depth plane wherever no block is present.
    /// </summary>
    /// <param name="keptBlocks">Running list of non-default blocks in plane.</param>
    /// <param name="depthPlane">Depth plane.</param>
    /// <param name="baseX">Base X coordinate.</param>
    /// <param name="baseY">Base Y coordinate.</param>
    /// <param name="baseZ">Base Z coordinate.</param>
    private void AddAirBlocks(List<BlockData> keptBlocks, DepthPlane depthPlane, 
        int baseX, int baseY, int baseZ)
    {
        // Determine where air blocks need to be added.
        var filledPoints = new bool[config.SegmentLength, config.SegmentLength];
        foreach (var block in depthPlane.Blocks)
        {
            int offsetX = block.Position.X - baseX;
            int offsetY = block.Position.Y - baseY;
            filledPoints[offsetX, offsetY] = true;
        }

        // Now add all of the air blocks.
        var airType = new BlockMaterialData { MaterialId = Material.Air, ModifierId = 0 };
        for (int i = 0; i < config.SegmentLength; ++i)
        {
            for (int j = 0; j < config.SegmentLength; ++j)
            {
                if (!filledPoints[i, j])
                {
                    var block = new BlockData()
                    {
                        BlockType = airType,
                        Position = new GridPosition()
                        {
                            X = baseX + i,
                            Y = baseY + j,
                            Z = baseZ + depthPlane.OffsetZ
                        }
                    };
                    keptBlocks.Add(block);
                }
            }
        }
    }

}

