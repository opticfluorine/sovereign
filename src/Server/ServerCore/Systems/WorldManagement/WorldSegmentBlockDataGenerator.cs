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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;
using EntityConstants = Sovereign.EngineCore.Entities.EntityConstants;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for creating and updating summary world segment block data structures.
/// </summary>
public sealed class WorldSegmentBlockDataGenerator
{
    private readonly BlockWorldSegmentIndexer blockIndexer;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly EntityTable entityTable;
    private readonly ILogger<WorldSegmentBlockDataGenerator> logger;
    private readonly WorldSegmentResolver resolver;

    public WorldSegmentBlockDataGenerator(WorldSegmentResolver resolver, EntityTable entityTable,
        BlockWorldSegmentIndexer blockIndexer,
        BlockPositionComponentCollection blockPositions,
        ILogger<WorldSegmentBlockDataGenerator> logger)
    {
        this.resolver = resolver;
        this.entityTable = entityTable;
        this.blockIndexer = blockIndexer;
        this.blockPositions = blockPositions;
        this.logger = logger;
    }

    /// <summary>
    ///     Creates summary block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Summary block data.</returns>
    public WorldSegmentBlockData Create(GridPosition segmentIndex)
    {
        // Retrieve the latest data for the requested world segment.
        var blocks = blockIndexer.GetEntitiesInWorldSegment(segmentIndex);
        var positionedBlocks = blocks
            .Select(entityId => Tuple.Create(entityId, blockPositions[entityId])).ToList();

        logger.LogTrace("Segment {Index} has {Count} blocks.", segmentIndex, blocks.Count);

        // Retrieve material data, group into depth planes.
        var basePoint = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var depthPlanes = new DepthPlane[WorldConstants.SegmentLength];
        for (var i = 0; i < WorldConstants.SegmentLength; ++i)
            depthPlanes[i] = new DepthPlane(WorldConstants.SegmentLength, i);
        GroupByDepth(positionedBlocks, depthPlanes, (int)basePoint.Z);

        // Initialize a new block summary data object.
        var data = GetEmptyBlockData();

        // Process each depth plane separately.
        for (var i = 0; i < WorldConstants.SegmentLength; ++i)
        {
            // Update defaults.
            data.DefaultsPerPlane[i] = depthPlanes[i].DefaultBlockType;

            // Add the layer if it contains non-default blocks.
            if (TryConvertDepthPlane(depthPlanes[i],
                    (int)basePoint.X, (int)basePoint.Y, (int)basePoint.Z,
                    out var converted))
                // At least one non-default block in plane.
                data.DataPlanes.Add(converted);
        }

        return data;
    }

    /// <summary>
    ///     Gets an empty block data object for use.
    /// </summary>
    /// <returns>Empty block data object.</returns>
    private WorldSegmentBlockData GetEmptyBlockData()
    {
        var data = new WorldSegmentBlockData();
        data.DefaultsPerPlane =
            new EngineCore.Systems.WorldManagement.BlockData[WorldConstants.SegmentLength];
        data.DataPlanes = new List<WorldSegmentBlockDataPlane>();

        return data;
    }

    /// <summary>
    ///     Groups a list of block entities into depth planes while also retrieving material data.
    ///     This method also identifies the most frequent material appearing in each plane.
    /// </summary>
    /// <param name="blocks">Blocks in the world segment.</param>
    /// <param name="depthPlanes">Depth planes to populate, indexed by z offset from segment origin.</param>
    /// <param name="baseZ">Z coordinate of segment base point.</param>
    private void GroupByDepth(List<Tuple<ulong, GridPosition>> blocks, DepthPlane[] depthPlanes, int baseZ)
    {
        // Group blocks by depth.
        foreach (var block in blocks)
        {
            var offset = block.Item2.Z - baseZ;
            if (!entityTable.TryGetTemplate(block.Item1, out var templateEntityId))
            {
                logger.LogWarning("No template for block ID {Id:X}, skipping.", block.Item1);
                continue;
            }

            var blockData = new BlockData
            {
                Data = new EngineCore.Systems.WorldManagement.BlockData
                {
                    BlockType = BlockDataType.Template,
                    TemplateIdOffset = templateEntityId - EntityConstants.FirstTemplateEntityId
                },
                Position = block.Item2
            };

            depthPlanes[offset].Add(blockData);
        }

        // Finish all depth planes.
        foreach (var plane in depthPlanes) plane.Finish();
    }

    /// <summary>
    ///     Converts a depth plane. If any non-default blocks are present in the
    ///     depth plane, a data plane is passed back.
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
        var lines = new WorldSegmentBlockDataLine[WorldConstants.SegmentLength];
        for (var i = 0; i < lines.Length; ++i)
            lines[i] = new WorldSegmentBlockDataLine
            {
                OffsetY = (byte)i,
                BlockData = new List<LinePositionedBlockData>()
            };

        // Filter to exclude default blocks.
        var keptBlocks = new List<BlockData>();
        foreach (var block in depthPlane.Blocks)
            if (!block.Data.Equals(depthPlane.DefaultBlockType))
                keptBlocks.Add(block);

        // As a special case, if the default block is not air, the air blocks
        // must be created and added to the kept set.
        if (depthPlane.DefaultBlockType.BlockType != BlockDataType.Air)
            AddAirBlocks(keptBlocks, depthPlane, baseX, baseY, baseZ);

        // Group blocks into data lines.
        foreach (var block in keptBlocks)
        {
            // Convert block.
            var positionedBlock = new LinePositionedBlockData
            {
                OffsetX = (byte)(block.Position.X - baseX),
                Data = block.Data
            };

            var offsetY = block.Position.Y - baseY;
            lines[offsetY].BlockData.Add(positionedBlock);
        }

        // Retain only the non-empty data lines in the converted plane.
        converted = new WorldSegmentBlockDataPlane();
        converted.OffsetZ = (byte)depthPlane.OffsetZ;
        var hasLines = false;
        foreach (var line in lines)
            if (line.BlockData.Count > 0)
            {
                converted.Lines.Add(line);
                hasLines = true;
            }

        return hasLines;
    }

    /// <summary>
    ///     Generates explicit air blocks in the depth plane wherever no block is present.
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
        var filledPoints = new bool[WorldConstants.SegmentLength,
            WorldConstants.SegmentLength];
        foreach (var block in depthPlane.Blocks)
        {
            var offsetX = block.Position.X - baseX;
            var offsetY = block.Position.Y - baseY;
            filledPoints[offsetX, offsetY] = true;
        }

        // Now add all of the air blocks.
        var airType = new EngineCore.Systems.WorldManagement.BlockData
            { BlockType = BlockDataType.Air };
        for (var i = 0; i < WorldConstants.SegmentLength; ++i)
        for (var j = 0; j < WorldConstants.SegmentLength; ++j)
            if (!filledPoints[i, j])
            {
                var block = new BlockData
                {
                    Data = airType,
                    Position = new GridPosition
                    {
                        X = baseX + i,
                        Y = baseY + j,
                        Z = baseZ + depthPlane.OffsetZ
                    }
                };
                keptBlocks.Add(block);
            }
    }

    /// <summary>
    ///     Internal block data type.
    /// </summary>
    private class BlockData
    {
        /// <summary>
        ///     Block type.
        /// </summary>
        public EngineCore.Systems.WorldManagement.BlockData Data = new();

        /// <summary>
        ///     Block position.
        /// </summary>
        public GridPosition Position;
    }

    /// <summary>
    ///     Internal struct used for grouping blocks into depth planes.
    /// </summary>
    private class DepthPlane
    {
        /// <summary>
        ///     Blocks in the depth plane.
        /// </summary>
        public readonly List<BlockData> Blocks = new();

        /// <summary>
        ///     Running count of material types in this depth plane.
        /// </summary>
        private readonly Dictionary<EngineCore.Systems.WorldManagement.BlockData, int> materialCounts = new();

        private int airBlockCount;

        /// <summary>
        ///     Cached count of most frequent material type.
        /// </summary>
        private int leadingMaterialCount;

        /// <summary>
        ///     Creates an empty depth plane.
        /// </summary>
        /// <param name="sideLength">Length of a block side.</param>
        /// <param name="offsetZ">Z offset of this depth plane.</param>
        public DepthPlane(uint sideLength, int offsetZ)
        {
            OffsetZ = offsetZ;
            airBlockCount = (int)(sideLength * sideLength);
        }

        /// <summary>
        ///     Default block type for this depth plane.
        /// </summary>
        public EngineCore.Systems.WorldManagement.BlockData DefaultBlockType { get; private set; }
            = new() { BlockType = BlockDataType.Air };

        /// <summary>
        ///     Z offset of this depth plane.
        /// </summary>
        public int OffsetZ { get; }

        /// <summary>
        ///     Adds a block to the depth plane.
        /// </summary>
        /// <param name="block">Block.</param>
        public void Add(BlockData block)
        {
            Blocks.Add(block);
            airBlockCount--;

            // Update material counts.
            int count;
            if (materialCounts.TryGetValue(block.Data, out count))
                count++;
            else
                count = 1;
            materialCounts[block.Data] = count;

            // Update default block if needed.
            if (count > leadingMaterialCount)
            {
                leadingMaterialCount = count;
                DefaultBlockType = block.Data;
            }
        }

        /// <summary>
        ///     Finishes the depth plane once all blocks have been added.
        /// </summary>
        public void Finish()
        {
            // Check for case where air is the most common block type.
            if (airBlockCount > leadingMaterialCount)
                DefaultBlockType = new EngineCore.Systems.WorldManagement.BlockData
                    { BlockType = BlockDataType.Air };
        }
    }
}