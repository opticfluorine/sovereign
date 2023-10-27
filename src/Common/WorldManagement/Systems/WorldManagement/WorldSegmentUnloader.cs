/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Movement.Components.Indexers;
using Sovereign.WorldManagement.WorldSegments;

namespace Sovereign.WorldManagement.Systems.WorldManagement;

/// <summary>
///     Responsible for unloading discrete world segments.
/// </summary>
public sealed class WorldSegmentUnloader : IWorldSegmentUnloader
{
    /// <summary>
    ///     Reusable buffer for entity lookup.
    /// </summary>
    private readonly IList<PositionedEntity> entityBuffer = new List<PositionedEntity>();

    private readonly EntityManager entityManager;
    private readonly PositionComponentIndexer positionComponentIndexer;
    private readonly WorldSegmentRegistry worldSegmentRegistry;
    private readonly WorldSegmentResolver worldSegmentResolver;

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
        var (minPos, maxPos) = worldSegmentResolver
            .GetRangeForWorldSegment(segmentIndex);

        /* Get the entities in the range. */
        entityBuffer.Clear();
        using (var indexLock = positionComponentIndexer.AcquireLock())
        {
            positionComponentIndexer.GetEntitiesInRange(indexLock, minPos, maxPos, entityBuffer);
        }

        /* Unload the entities. */
        foreach (var entity in entityBuffer) entityManager.UnloadEntity(entity.EntityId);
        worldSegmentRegistry.OnSegmentUnloaded(segmentIndex);
    }
}