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
using System.Diagnostics.CodeAnalysis;

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

    /// <summary>
    ///     Flag indicating whether the sprite's origin is on the perspective line.
    /// </summary>
    public bool OriginOnLine;
}

/// <summary>
///     Represents a list of entities at a common z-depth.
/// </summary>
public class EntityList(int z, List<EntityInfo> entities)
{
    /// <summary>
    ///     Comparer for EntityLists.
    /// </summary>
    public static readonly IComparer<EntityList> Comparer =
        Comparer<EntityList>.Create((a, b) => Comparer<float>.Default.Compare(a.ZFloor, b.ZFloor));

    private static readonly List<EntityInfo> EmptyList = new(0);

    /// <summary>
    ///     Entities at this z-floor sorted in descending order.
    /// </summary>
    public readonly List<EntityInfo> Entities = entities;

    /// <summary>
    ///     Z floor of this entity list.
    /// </summary>
    public readonly int ZFloor = z;

    private static readonly List<int> ToRemove = new();

    /// <summary>
    ///     Constructs a new empty EntityList for lookups only.
    /// </summary>
    /// <param name="zFloor">Z floor.</param>
    private EntityList(int zFloor) : this(zFloor, EmptyList)
    {
    }

    /// <summary>
    ///     Convenience method to improve code readability when creating a dummy list as a comparison key.
    /// </summary>
    /// <param name="zFloor">Z floor.</param>
    /// <returns>Dummy list for use as a comparison key.</returns>
    public static EntityList ForComparison(int zFloor)
    {
        return new EntityList(zFloor);
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
    ///     Ordered (descending) list of entities on this line at each non-empty Z floor.
    /// </summary>
    public readonly List<EntityList> ZFloors = new();

    /// <summary>
    ///     Reference count.
    /// </summary>
    public uint ReferenceCount;

    /// <summary>
    ///     Gets the entity list for the given Z floor, if any.
    /// </summary>
    /// <param name="zFloor">Z floor.</param>
    /// <param name="zSet">Corresponding Z set, or null if not found.</param>
    /// <returns>true if found, false otherwise.</returns>
    public bool TryGetListForZFloor(int zFloor, [NotNullWhen(true)] out EntityList? zSet)
    {
        var index = ZFloors.BinarySearch(EntityList.ForComparison(zFloor), EntityList.Comparer);
        if (index < 0)
        {
            zSet = null;
            return false;
        }

        zSet = ZFloors[index];
        return true;
    }
}