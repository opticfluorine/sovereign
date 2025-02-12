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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.WorldManagement;

/// <summary>
///     Responsible for parsing WorldSegmentBlockData objects into entities.
/// </summary>
public class WorldSegmentBlockDataLoader
{
    private readonly BlockController blockController;
    private readonly IEventSender eventSender;
    private readonly ILogger<WorldSegmentBlockDataLoader> logger;
    private readonly WorldSegmentResolver resolver;

    public WorldSegmentBlockDataLoader(IEventSender eventSender, BlockController blockController,
        WorldSegmentResolver resolver,
        ILogger<WorldSegmentBlockDataLoader> logger)
    {
        this.eventSender = eventSender;
        this.blockController = blockController;
        this.resolver = resolver;
        this.logger = logger;
    }

    /// <summary>
    ///     Loads entities into a world segment from the given block data.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="segmentData">World segment block data.</param>
    public void Load(GridPosition segmentIndex, WorldSegmentBlockData segmentData)
    {
        // Validate input.
        if (segmentData.DefaultsPerPlane.Length != WorldManagementConfiguration.SegmentLength)
            throw new RankException("Bad number of default materials in segment data.");

        // Load and create block entities.
        blockController.AddBlocksForWorldSegment(eventSender,
            blocksToAdd => CreateBlocksToAdd(segmentIndex, segmentData, blocksToAdd),
            segmentIndex);
    }

    /// <summary>
    ///     Creates block records from segment data and adds them to the given list.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="segmentData">Segment data.</param>
    /// <param name="blocksToAdd">Running list of blocks to add.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if a depth plane has a bad Z offset.</exception>
    private void CreateBlocksToAdd(GridPosition segmentIndex, WorldSegmentBlockData segmentData,
        IList<BlockRecord> blocksToAdd)
    {
        /* Start by processing depth plaens containing non-default blocks. */
        var processed = new bool[WorldManagementConfiguration.SegmentLength];
        foreach (var plane in segmentData.DataPlanes)
        {
            // Validation.
            if (plane.OffsetZ >= WorldManagementConfiguration.SegmentLength)
                throw new IndexOutOfRangeException("Bad Z-offset in world segment data plane.");

            processed[plane.OffsetZ] = true;
            ProcessMixedPlane(segmentIndex, plane, segmentData.DefaultsPerPlane[plane.OffsetZ], blocksToAdd);
        }

        /* Process the remaining all-default planes. */
        for (var i = 0; i < processed.Length; i++)
            if (!processed[i] && segmentData.DefaultsPerPlane[i].BlockType != BlockDataType.Air)
                ProcessDefaultPlane(segmentIndex, i, segmentData.DefaultsPerPlane[i], blocksToAdd);

        if (blocksToAdd.Count > 0)
            logger.LogDebug("Adding {0} blocks for world segment {1}.", blocksToAdd.Count, segmentIndex);
    }

    /// <summary>
    ///     Processes a plane containing only default blocks.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="offsetZ">Z offset of the plane.</param>
    /// <param name="defaultBlock">Default block type.</param>
    /// <param name="blocksToAdd">List of blocks to add.</param>
    private void ProcessDefaultPlane(GridPosition segmentIndex, int offsetZ, BlockData defaultBlock,
        IList<BlockRecord> blocksToAdd)
    {
        var basePosition = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var baseX = (int)basePosition.X;
        var baseY = (int)basePosition.Y;
        var baseZ = (int)basePosition.Z;

        for (var x = 0; x < WorldManagementConfiguration.SegmentLength; x++)
        for (var y = 0; y < WorldManagementConfiguration.SegmentLength; y++)
        {
            var block = new BlockRecord
            {
                Position = new GridPosition(baseX + x, baseY + y, baseZ + offsetZ),
                TemplateEntityId = defaultBlock.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
            };
            blocksToAdd.Add(block);
        }
    }

    /// <summary>
    ///     Processes a plane containing non-default blocks.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="plane">Depth plane to process.</param>
    /// <param name="defaultBlock">Default block type.</param>
    /// <param name="blocksToAdd">List of blocks to add.</param>
    private void ProcessMixedPlane(GridPosition segmentIndex, WorldSegmentBlockDataPlane plane,
        BlockData defaultBlock, IList<BlockRecord> blocksToAdd)
    {
        var basePosition = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var baseX = (int)basePosition.X;
        var baseY = (int)basePosition.Y;
        var baseZ = (int)basePosition.Z;

        /* Process the non-default blocks. */
        var nonDefaults = new bool[WorldManagementConfiguration.SegmentLength,
            WorldManagementConfiguration.SegmentLength];
        foreach (var line in plane.Lines)
        {
            /* Validate. */
            if (line.OffsetY >= WorldManagementConfiguration.SegmentLength)
                throw new IndexOutOfRangeException("Bad Y-offset in world segment data line.");

            foreach (var block in line.BlockData)
            {
                /* Validate. */
                if (block.OffsetX >= WorldManagementConfiguration.SegmentLength)
                    throw new IndexOutOfRangeException("Bad X-offset in world segment data block.");

                var x = baseX + block.OffsetX;
                var y = baseY + line.OffsetY;
                var z = baseZ + plane.OffsetZ;

                /* Add the block. */
                var blockRecord = new BlockRecord
                {
                    Position = new GridPosition(x, y, z),
                    TemplateEntityId = block.Data.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
                };
                blocksToAdd.Add(blockRecord);

                nonDefaults[block.OffsetX, line.OffsetY] = true;
            }
        }

        /* Fill in the default blocks (unless they are air). */
        if (defaultBlock.BlockType != BlockDataType.Air)
            for (var i = 0; i < WorldManagementConfiguration.SegmentLength; ++i)
            for (var j = 0; j < WorldManagementConfiguration.SegmentLength; ++j)
                if (!nonDefaults[i, j])
                {
                    var blockRecord = new BlockRecord
                    {
                        Position = new GridPosition(baseX + i, baseY + j, baseZ + plane.OffsetZ),
                        TemplateEntityId = defaultBlock.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
                    };
                    blocksToAdd.Add(blockRecord);
                }
    }
}