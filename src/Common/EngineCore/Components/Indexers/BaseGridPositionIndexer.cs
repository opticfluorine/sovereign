/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Base class for component indexers that allow lookup by exact integer
///     grid coordinates.
/// </summary>
public class BaseGridPositionIndexer : BaseComponentIndexer<GridPosition>
{
    private readonly Dictionary<GridPosition, HashSet<ulong>> entitiesByPosition = new();

    private readonly Dictionary<ulong, GridPosition> knownPositions = new();

    private readonly ILogger logger;

    protected BaseGridPositionIndexer(BaseComponentCollection<GridPosition> components,
        IComponentEventSource<GridPosition> eventSource, ILogger logger)
        : base(components, eventSource)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Gets the entities at the given grid position.
    /// </summary>
    /// <param name="position">Position to query.</param>
    /// <returns>The set of entities at the position, or null if there are none.</returns>
    public HashSet<ulong>? GetEntitiesAtPosition(GridPosition position)
    {
        return entitiesByPosition.TryGetValue(position, out var entities)
            ? entities
            : null;
    }

    protected override void ComponentAddedCallback(ulong entityId, GridPosition componentValue, bool isLoad)
    {
        AddEntity(entityId, componentValue);
    }

    protected override void ComponentModifiedCallback(ulong entityId, GridPosition componentValue)
    {
        RemoveEntity(entityId);
        AddEntity(entityId, componentValue);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        RemoveEntity(entityId);
    }

    /// <summary>
    ///     Adds the given entity to the index.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddEntity(ulong entityId, GridPosition position)
    {
        var set = GetSetForPosition(position);
        set.Add(entityId);
        knownPositions[entityId] = position;
    }

    /// <summary>
    ///     Gets the set for the given grid position, creating it if necessary.
    /// </summary>
    /// <param name="position">Grid position.</param>
    /// <returns>Set.</returns>
    private HashSet<ulong> GetSetForPosition(GridPosition position)
    {
        var exists = entitiesByPosition.TryGetValue(position, out var set);
        if (!exists || set == null)
        {
            set = new HashSet<ulong>();
            entitiesByPosition[position] = set;
        }

        return set;
    }

    /// <summary>
    ///     Removes the given entity from the index.
    /// </summary>
    /// <param name="entityId">Entity ID to remove.</param>
    private void RemoveEntity(ulong entityId)
    {
        if (!knownPositions.TryGetValue(entityId, out var gridPos))
        {
            logger.ErrorFormat("Could not find entity {0:X} to remove.", entityId);
            return;
        }

        var set = entitiesByPosition[gridPos];

        set.Remove(entityId);
        knownPositions.Remove(entityId);
    }
}