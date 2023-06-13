// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.ServerCore.Events;
using Sovereign.WorldManagement.Configuration;
using Sovereign.WorldManagement.WorldSegments;

namespace Sovereign.ServerCore.Systems.Debug;

/// <summary>
/// Handles events received by the debug system.
/// </summary>
public sealed class DebugEventHandler
{
    private readonly WorldSegmentResolver worldSegmentResolver;
    private readonly IWorldManagementConfiguration worldManagementConfig;
    private readonly BlockController blockController;
    private IEventSender eventSender;

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
    /// Handles a debug command event.
    /// </summary>
    /// <param name="details">Command details.</param>
    public void HandleDebugCommand(DebugCommandEventDetails details)
    {
        switch (details.Command.Type)
        {
            case DebugCommandType.GenerateWorldData:
                DoGenerateWorldData();
                break;
            
            default:
                break;
        }
    }

    /// <summary>
    /// Processes a debug command to generate test world data.
    /// </summary>
    private void DoGenerateWorldData()
    {
        blockController.AddBlocks(eventSender, 
            (blocks) => CreateSegmentedWorldData(blocks));
    }

    /// <summary>
    /// Creates a series of world segments with test data.
    /// </summary>
    /// <param name="blocks">Array to hold new blocks.</param>
    private void CreateSegmentedWorldData(IList<BlockRecord> blocks)
    {
        for (int i = -2; i < 3; ++i)
        {
            for (int j = -2; j < 3; ++j)
            {
                CreateBlocksForSegment(blocks, new GridPosition(i, j, 0));
            }
        }
    }
    
    /// <summary>
    /// Creates the test blocks for a world segment.
    /// </summary>
    /// <param name="blocks">List to hold created blocks.</param>
    /// <param name="segmentIndex">World segment index.</param>
    private void CreateBlocksForSegment(IList<BlockRecord> blocks,
        GridPosition segmentIndex)
    {
        var origin = (GridPosition)worldSegmentResolver
            .GetRangeForWorldSegment(segmentIndex).Item1;
        for (int i = 0; i < worldManagementConfig.SegmentLength; ++i)
        {
            for (int j = 0; j < worldManagementConfig.SegmentLength; ++j)
            {
                var pos = new GridPosition(
                    origin.X + i,
                    origin.Y + j,
                    origin.Z);
                blocks.Add(new BlockRecord()
                {
                    Position = pos,
                    Material = 2 + ((i % 2 + j) % 2),
                    MaterialModifier = 0
                });
            }
        }
    }
}