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
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Movement.Components.Indexers;
using Sovereign.WorldManagement.WorldSegments;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.WorldManagement.Systems.WorldManagement
{

    /// <summary>
    /// Responsible for unloading discrete world segments.
    /// </summary>
    public sealed class WorldSegmentUnloader : IWorldSegmentUnloader
    {
        private readonly EntityManager entityManager;
        private readonly PositionComponentIndexer positionComponentIndexer;
        private readonly WorldSegmentResolver worldSegmentResolver;
        private readonly WorldSegmentRegistry worldSegmentRegistry;

        /// <summary>
        /// Reusable buffer for entity lookup.
        /// </summary>
        private readonly IList<PositionedEntity> entityBuffer = new List<PositionedEntity>();

        public WorldSegmentUnloader(EntityManager entityManager,
            PositionComponentIndexer positionComponentIndexer,
            WorldSegmentResolver worldSegmentResolver,
            WorldSegmentRegistry worldSegmentRegistry)
        {
            this.entityManager = entityManager;
            this.positionComponentIndexer = positionComponentIndexer;
            this.worldSegmentResolver = worldSegmentResolver;
            this.worldSegmentRegistry = worldSegmentRegistry;
        }

        public void UnloadSegment(GridPosition segmentIndex)
        {
            /* Get the position range to be unloaded. */
            (var minPos, var maxPos) = worldSegmentResolver
                .GetRangeForWorldSegment(segmentIndex);

            /* Get the entities in the range. */
            entityBuffer.Clear();
            using (var indexLock = positionComponentIndexer.AcquireLock())
            {
                positionComponentIndexer.GetEntitiesInRange(indexLock, minPos, maxPos, entityBuffer);
            }

            /* Unload the entities. */
            foreach (var entity in entityBuffer)
            {
                entityManager.UnloadEntity(entity.EntityId);
            }
            worldSegmentRegistry.OnSegmentUnloaded(segmentIndex);
        }

    }

}
