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

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Entity types that can be found on a perspective line.
/// </summary>
public enum EntityType
{
    /// <summary>
    ///     Non-block entity.
    /// </summary>
    NonBlock,

    /// <summary>
    ///     Front face of a block entity.
    /// </summary>
    BlockFrontFace,

    /// <summary>
    ///     Top face of a block entity.
    /// </summary>
    BlockTopFace
}

/// <summary>
///     Information regarding an entity on a perspective line.
/// </summary>
public struct EntityInfo
{
    /// <summary>
    ///     Entity ID.
    /// </summary>
    public ulong EntityId;

    /// <summary>
    ///     Entity type.
    /// </summary>
    public EntityType EntityType;
}

/// <summary>
///     Represents a list of entities at a common z-depth.
/// </summary>
public readonly struct EntityList(float z, List<EntityInfo> entities)
{
    /// <summary>
    ///     Comparer for EntityLists.
    /// </summary>
    public static readonly IComparer<EntityList> Comparer =
        Comparer<EntityList>.Create((a, b) => Comparer<float>.Default.Compare(a.z, b.z));

    private static readonly List<EntityInfo> EmptyList = new(0);

    /// <summary>
    ///     Entities at this z-depth.
    /// </summary>
    public readonly List<EntityInfo> Entities = entities;

    /// <summary>
    ///     Z depth of this entity list.
    /// </summary>
    private readonly float z = z;

    private static readonly List<int> ToRemove = new();

    /// <summary>
    ///     Constructs a new empty EntityList for lookups only.
    /// </summary>
    /// <param name="z">Z depth.</param>
    private EntityList(float z) : this(z, EmptyList)
    {
    }

    /// <summary>
    ///     Convenience method to improve code readability when creating a dummy list as a comparison key.
    /// </summary>
    /// <param name="z">Z depth.</param>
    /// <returns>Dummy list for use as a comparison key.</returns>
    public static EntityList ForComparison(float z)
    {
        return new EntityList(z);
    }

    /// <summary>
    ///     Removes all occurrences of an entity in this list.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void RemoveEntity(ulong entityId)
    {
        ToRemove.Clear();
        for (var i = 0; i < Entities.Count; ++i)
            if (Entities[i].EntityId == entityId)
                ToRemove.Add(i);

        for (var i = ToRemove.Count - 1; i >= 0; --i) Entities.RemoveAt(ToRemove[i]);
    }
}

/// <summary>
///     Tracks the entities which overlap a single perspective line.
/// </summary>
public class PerspectiveLine
{
    /// <summary>
    ///     Z values at which entities are located on this perspective line.
    /// </summary>
    public readonly SortedSet<EntityList> ZDepths = new(EntityList.Comparer);

    /// <summary>
    ///     Reference count.
    /// </summary>
    public uint ReferenceCount;
}