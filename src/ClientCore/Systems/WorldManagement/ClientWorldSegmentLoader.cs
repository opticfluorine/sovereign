/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.WorldManagement.Configuration;
using Sovereign.WorldManagement.Systems.WorldManagement;
using Sovereign.WorldManagement.WorldSegments;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.WorldManagement
{

    /// <summary>
    /// Client-side world segment loader.
    /// </summary>
    public sealed class ClientWorldSegmentLoader : IWorldSegmentLoader
    {
        private readonly IEventSender eventSender;
        private readonly WorldSegmentResolver worldSegmentResolver;
        private readonly WorldSegmentRegistry worldSegmentRegistry;
        private readonly IWorldManagementConfiguration config;
        private readonly BlockController blockController;

        public ClientWorldSegmentLoader(IEventSender eventSender,
            WorldSegmentResolver worldSegmentResolver,
            WorldSegmentRegistry worldSegmentRegistry,
            IWorldManagementConfiguration config,
            BlockController blockController)
        {
            this.eventSender = eventSender;
            this.worldSegmentResolver = worldSegmentResolver;
            this.worldSegmentRegistry = worldSegmentRegistry;
            this.config = config;
            this.blockController = blockController;
        }

        public void LoadSegment(GridPosition segmentIndex)
        {
            /* Fill the segment with a known block pattern for test. */
            blockController.AddBlocks(eventSender,
                (blocks) => CreateBlocks(blocks, segmentIndex));
            worldSegmentRegistry.OnSegmentLoaded(segmentIndex);
        }

        /// <summary>
        /// Creates the test blocks for a world segment.
        /// </summary>
        /// <param name="blocks">List to hold created blocks.</param>
        /// <param name="segmentIndex">World segment index.</param>
        private void CreateBlocks(IList<BlockRecord> blocks,
            GridPosition segmentIndex)
        {
            var origin = (GridPosition)worldSegmentResolver
                .GetRangeForWorldSegment(segmentIndex).Item1;
            for (int i = 0; i < config.SegmentLength; ++i)
            {
                for (int j = 0; j < config.SegmentLength; ++j)
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
}
