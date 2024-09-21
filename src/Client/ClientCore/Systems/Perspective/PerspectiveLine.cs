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
    ///     Z coordinate.
    /// </summary>
    public float Z;

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
    /// <remarks>
    ///     The order of the comparison is reversed since the Z floors are sorted in descending
    ///     order within a perspective line.
    /// </remarks>
    public static readonly IComparer<EntityList> Comparer =
        Comparer<EntityList>.Create((a, b) => Comparer<float>.Default.Compare(b.ZFloor, a.ZFloor));

    private static readonly List<EntityInfo> EmptyList = new(0);

    private static readonly List<int> ToRemove = new();

    /// <summary>
    ///     Entities at this z-floor sorted in descending order.
    /// </summary>
    public readonly List<EntityInfo> Entities = entities;

    /// <summary>
    ///     Z floor of this entity list.
    /// </summary>
    public readonly int ZFloor = z;

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
    ///     Adds an entity to the list.
    /// </summary>
    /// <param name="entityInfo">Entity info.</param>
    public void AddEntity(EntityInfo entityInfo)
    {
        // Find position to insert.
        // This will generally be a short list, so directly iterate instead of binary search here.
        var index = 0;
        for (var i = 0; i < Entities.Count; ++i)
            if (Entities[i].Z < entityInfo.Z)
            {
                index = i;
                break;
            }

        Entities.Insert(index, entityInfo);
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

    /// <summary>
    ///     Adds an EntityList at the given z floor.
    /// </summary>
    /// <param name="zFloor">Z floor.</param>
    /// <param name="zSet">EntityList to add.</param>
    public void AddZFloor(int zFloor, EntityList zSet)
    {
        var index = ~ZFloors.BinarySearch(EntityList.ForComparison(zFloor), EntityList.Comparer);
        if (index < 0) return;

        ZFloors.Insert(index, zSet);
    }

    /// <summary>
    ///     Removes the given z floor from the line.
    /// </summary>
    /// <param name="zFloor">z floor to remove.</param>
    public void RemoveZFloor(int zFloor)
    {
        var index = ZFloors.BinarySearch(EntityList.ForComparison(zFloor), EntityList.Comparer);
        if (index < 0) return;

        ZFloors.RemoveAt(index);
    }
}