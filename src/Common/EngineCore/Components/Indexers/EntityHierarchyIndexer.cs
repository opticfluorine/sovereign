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

using System.Collections.Generic;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Component indexer that tracks the full hierarchy of parent-child relationships.
/// </summary>
public class EntityHierarchyIndexer : BaseComponentIndexer<ulong>
{
    /// <summary>
    ///     Map from entity to all of its descendants.
    /// </summary>
    private readonly Dictionary<ulong, HashSet<ulong>> allDescendants = new();

    /// <summary>
    ///     Map from entity to its direct children.
    /// </summary>
    private readonly Dictionary<ulong, HashSet<ulong>> directChildren = new();

    /// <summary>
    ///     Map from entity to the currently tracked parent entity.
    /// </summary>
    private readonly Dictionary<ulong, ulong> trackedParent = new();

    public EntityHierarchyIndexer(ParentComponentCollection parents)
        : base(parents, parents)
    {
    }

    /// <summary>
    ///     Gets the set of direct children of an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Set (possibly empty) of direct children of the entity.</returns>
    /// <remarks>
    ///     This method returns a copy of the set of entities, and ownership of
    ///     the set is transferred to the caller. It is only guaranteed to be
    ///     current at the moment the method was called, and only if the method
    ///     was called during normal processing of a tick (i.e. not during the
    ///     component update step between ticks).
    /// </remarks>
    public HashSet<ulong> GetDirectChildren(ulong entityId)
    {
        if (directChildren.TryGetValue(entityId, out var children))
            return new HashSet<ulong>(children);

        return new HashSet<ulong>();
    }

    /// <summary>
    ///     Gets the set of all descendants of an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Set (possibly empty) of all descendants of the entity.</returns>
    /// <remarks>
    ///     This method returns a copy of the set of entities, and ownership of
    ///     the set is transferred to the caller. It is only guaranteed to be
    ///     current at the moment the method was called, and only if the method
    ///     was called during normal processing of a tick (i.e. not during the
    ///     component update step between ticks).
    /// </remarks>
    public HashSet<ulong> GetAllDescendants(ulong entityId)
    {
        if (allDescendants.TryGetValue(entityId, out var children))
            return new HashSet<ulong>(children);

        return new HashSet<ulong>();
    }

    /// <summary>
    ///     Adds the set of all descendants of an entity to the given list.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="descendants">List to populate with descendants.</param>
    public void GetAllDescendants(ulong entityId, List<ulong> descendants)
    {
        if (!allDescendants.TryGetValue(entityId, out var children)) return;
        descendants.AddRange(children);
    }

    protected override void ComponentAddedCallback(ulong entityId, ulong parentId, bool isLoad)
    {
        AddEntity(entityId, parentId);
    }

    protected override void ComponentModifiedCallback(ulong entityId, ulong parentId)
    {
        // Do this cleanly by removing the existing relationships, then adding
        // in the new relationships.
        RemoveEntity(entityId);
        AddEntity(entityId, parentId);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        RemoveEntity(entityId);
    }

    /// <summary>
    ///     Adds an entity to the trees.
    /// </summary>
    /// <param name="entityId">Entity.</param>
    /// <param name="parentId">Parent.</param>
    private void AddEntity(ulong entityId, ulong parentId)
    {
        // Add the entity to its direct parent.
        if (!directChildren.ContainsKey(parentId))
            directChildren[parentId] = new HashSet<ulong>();
        if (!allDescendants.ContainsKey(parentId))
            allDescendants[parentId] = new HashSet<ulong>();
        if (!allDescendants.ContainsKey(entityId))
            allDescendants[entityId] = new HashSet<ulong>();

        directChildren[parentId].Add(entityId);
        allDescendants[parentId].Add(entityId);

        // Add the entity to the full span of any entities above the parent.
        // Also add any children of the entity to the full span in case a
        // new linkage was added in the middle of the tree.
        var currentId = parentId;
        var childSpan = allDescendants[entityId];
        while (trackedParent.TryGetValue(currentId, out currentId))
        {
            if (!allDescendants.ContainsKey(currentId))
                allDescendants[currentId] = new HashSet<ulong>();

            allDescendants[currentId].Add(entityId);
            allDescendants[currentId].UnionWith(childSpan);
        }

        trackedParent[entityId] = parentId;
    }

    /// <summary>
    ///     Removes an entity from the tree.
    /// </summary>
    /// <param name="entityId">Entity.</param>
    private void RemoveEntity(ulong entityId)
    {
        // Remove from its direct parent.
        var parentId = trackedParent[entityId];
        directChildren[parentId].Remove(entityId);
        allDescendants[parentId].Remove(entityId);

        // Remove from full span of any entities above the parent.
        // Also remove any descendants of the entity.
        var childSpan = allDescendants[entityId];
        while (trackedParent.TryGetValue(parentId, out parentId))
        {
            allDescendants[parentId].Remove(entityId);
            foreach (var childId in childSpan) allDescendants[parentId].Remove(childId);
        }

        trackedParent.Remove(entityId, out _);
    }
}