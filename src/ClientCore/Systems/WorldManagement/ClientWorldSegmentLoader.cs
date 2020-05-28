/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
