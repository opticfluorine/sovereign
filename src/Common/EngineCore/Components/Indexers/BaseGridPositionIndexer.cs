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
using System.Numerics;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Base class for component indexers that allow lookup by exact integer
///     grid coordinates.
/// </summary>
public class BaseGridPositionIndexer : BaseComponentIndexer<Vector3>
{
    private readonly IDictionary<GridPosition, ISet<ulong>> entitiesByPosition
        = new Dictionary<GridPosition, ISet<ulong>>();

    private readonly IDictionary<ulong, GridPosition> knownPositions
        = new Dictionary<ulong, GridPosition>();

    protected BaseGridPositionIndexer(BaseComponentCollection<Vector3> components,
        IComponentEventSource<Vector3> eventSource)
        : base(components, eventSource)
    {
    }

    /// <summary>
    ///     Gets the entities at the given grid position.
    /// </summary>
    /// <param name="position">Position to query.</param>
    /// <returns>The set of entities at the position, or null if there are none.</returns>
    public ISet<ulong>? GetEntitiesAtPosition(GridPosition position)
    {
        return entitiesByPosition.TryGetValue(position, out var entities)
            ? entities
            : null;
    }

    protected override void ComponentAddedCallback(ulong entityId, Vector3 componentValue, bool isLoad)
    {
        AddEntity(entityId, componentValue);
    }

    protected override void ComponentModifiedCallback(ulong entityId, Vector3 componentValue)
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
    private void AddEntity(ulong entityId, Vector3 position)
    {
        var gridPos = new GridPosition(position);
        var set = GetSetForPosition(gridPos);
        set.Add(entityId);
        knownPositions[entityId] = gridPos;
    }

    /// <summary>
    ///     Gets the set for the given grid position, creating it if necessary.
    /// </summary>
    /// <param name="position">Grid position.</param>
    /// <returns>Set.</returns>
    private ISet<ulong> GetSetForPosition(GridPosition position)
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
        if (!knownPositions.ContainsKey(entityId)) return;
        var gridPos = knownPositions[entityId];
        var set = entitiesByPosition[gridPos];

        set.Remove(entityId);
        knownPositions.Remove(entityId);
    }
}