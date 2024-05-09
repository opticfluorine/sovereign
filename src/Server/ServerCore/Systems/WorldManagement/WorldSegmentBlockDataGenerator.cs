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

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for creating and updating summary world segment block data structures.
/// </summary>
public sealed class WorldSegmentBlockDataGenerator
{
    private readonly IWorldManagementConfiguration config;
    private readonly BlockPositionIndexer indexer;
    private readonly MaterialComponentCollection materials;
    private readonly MaterialModifierComponentCollection modifiers;
    private readonly WorldSegmentResolver resolver;

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

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Creates summary block data for the given world segment.
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

        Logger.DebugFormat("Segment {0} has {1} blocks.", segmentIndex, blocks.Count);

        // Retrieve material data, group into depth planes.
        var basePoint = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var depthPlanes = new DepthPlane[config.SegmentLength];
        for (var i = 0; i < config.SegmentLength; ++i) depthPlanes[i] = new DepthPlane(config.SegmentLength, i);
        GroupByDepth(blocks, depthPlanes, (int)basePoint.Z);

        // Initialize a new block summary data object.
        var data = GetEmptyBlockData();

        // Process each depth plane separately.
        for (var i = 0; i < config.SegmentLength; ++i)
        {
            // Update defaults.
            data.DefaultMaterialsPerPlane[i] = depthPlanes[i].DefaultBlockType;

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
        data.DefaultMaterialsPerPlane = new BlockMaterialData[config.SegmentLength];
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
    private void GroupByDepth(List<PositionedEntity> blocks, DepthPlane[] depthPlanes, int baseZ)
    {
        // Group blocks by depth.
        foreach (var block in blocks)
        {
            var offset = (int)block.Position.Z - baseZ;
            var material = materials[block.EntityId];
            var modifier = modifiers[block.EntityId];

            var blockData = new BlockData
            {
                BlockType = new BlockMaterialData { MaterialId = material, ModifierId = modifier },
                Position = new GridPosition(block.Position)
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
        var lines = new WorldSegmentBlockDataLine[config.SegmentLength];
        for (var i = 0; i < lines.Length; ++i)
            lines[i] = new WorldSegmentBlockDataLine
            {
                OffsetY = (byte)i,
                BlockData = new List<LinePositionedBlockData>()
            };

        // Filter to exclude default blocks.
        var keptBlocks = new List<BlockData>();
        foreach (var block in depthPlane.Blocks)
            if (!block.BlockType.Equals(depthPlane.DefaultBlockType))
                keptBlocks.Add(block);

        // As a special case, if the default block is not air, the air blocks
        // must be created and added to the kept set.
        if (depthPlane.DefaultBlockType.MaterialId != MaterialConstants.Air)
            AddAirBlocks(keptBlocks, depthPlane, baseX, baseY, baseZ);

        // Group blocks into data lines.
        foreach (var block in keptBlocks)
        {
            // Convert block.
            var positionedBlock = new LinePositionedBlockData
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
        var filledPoints = new bool[config.SegmentLength, config.SegmentLength];
        foreach (var block in depthPlane.Blocks)
        {
            var offsetX = block.Position.X - baseX;
            var offsetY = block.Position.Y - baseY;
            filledPoints[offsetX, offsetY] = true;
        }

        // Now add all of the air blocks.
        var airType = new BlockMaterialData { MaterialId = MaterialConstants.Air, ModifierId = 0 };
        for (var i = 0; i < config.SegmentLength; ++i)
        for (var j = 0; j < config.SegmentLength; ++j)
            if (!filledPoints[i, j])
            {
                var block = new BlockData
                {
                    BlockType = airType,
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
        public BlockMaterialData BlockType = new();

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
        private readonly Dictionary<BlockMaterialData, int> materialCounts = new();

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
        public BlockMaterialData DefaultBlockType { get; private set; }
            = new() { MaterialId = MaterialConstants.Air, ModifierId = 0 };

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
            if (materialCounts.TryGetValue(block.BlockType, out count))
                count++;
            else
                count = 1;
            materialCounts[block.BlockType] = count;

            // Update default block if needed.
            if (count > leadingMaterialCount)
            {
                leadingMaterialCount = count;
                DefaultBlockType = block.BlockType;
            }
        }

        /// <summary>
        ///     Finishes the depth plane once all blocks have been added.
        /// </summary>
        public void Finish()
        {
            // Check for case where air is the most common block type.
            if (airBlockCount > leadingMaterialCount)
                DefaultBlockType = new BlockMaterialData { MaterialId = MaterialConstants.Air, ModifierId = 0 };
        }
    }
}