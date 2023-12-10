// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineCore.World;
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerCore.Systems.Debug;

/// <summary>
///     Handles events received by the debug system.
/// </summary>
public sealed class DebugEventHandler
{
    private readonly BlockController blockController;
    private readonly IEventSender eventSender;
    private readonly IWorldManagementConfiguration worldManagementConfig;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public DebugEventHandler(WorldSegmentResolver worldSegmentResolver,
        IWorldManagementConfiguration worldManagementConfig,
        BlockController blockController,
        IEventSender eventSender)
    {
        this.worldSegmentResolver = worldSegmentResolver;
        this.worldManagementConfig = worldManagementConfig;
        this.blockController = blockController;
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Handles a debug command event.
    /// </summary>
    /// <param name="details">Command details.</param>
    public void HandleDebugCommand(DebugCommandEventDetails details)
    {
        switch (details.Command.Type)
        {
            case DebugCommandType.GenerateWorldData:
                DoGenerateWorldData();
                break;
        }
    }

    /// <summary>
    ///     Processes a debug command to generate test world data.
    /// </summary>
    private void DoGenerateWorldData()
    {
        blockController.AddBlocks(eventSender,
            blocks => CreateSegmentedWorldData(blocks));
    }

    /// <summary>
    ///     Creates a series of world segments with test data.
    /// </summary>
    /// <param name="blocks">Array to hold new blocks.</param>
    private void CreateSegmentedWorldData(IList<BlockRecord> blocks)
    {
        for (var i = -2; i < 3; ++i)
        for (var j = -2; j < 3; ++j)
            CreateBlocksForSegment(blocks, new GridPosition(i, j, 0));
    }

    /// <summary>
    ///     Creates the test blocks for a world segment.
    /// </summary>
    /// <param name="blocks">List to hold created blocks.</param>
    /// <param name="segmentIndex">World segment index.</param>
    private void CreateBlocksForSegment(IList<BlockRecord> blocks,
        GridPosition segmentIndex)
    {
        var origin = (GridPosition)worldSegmentResolver
            .GetRangeForWorldSegment(segmentIndex).Item1;
        for (var i = 0; i < worldManagementConfig.SegmentLength; ++i)
        for (var j = 0; j < worldManagementConfig.SegmentLength; ++j)
        {
            var pos = new GridPosition(
                origin.X + i,
                origin.Y + j,
                origin.Z);
            blocks.Add(new BlockRecord
            {
                Position = pos,
                Material = 2 + (i % 2 + j) % 2,
                MaterialModifier = 0
            });
        }
    }
}