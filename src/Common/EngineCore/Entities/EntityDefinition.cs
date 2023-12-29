// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using System.Numerics;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Consolidated definition of an entity that can be used to transport and
///     reconstruct an entity over the network.
/// </summary>
public class EntityDefinition
{
    /// <summary>
    ///     Entity ID. Required.
    /// </summary>
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Position component, or null if entity is not positioned.
    /// </summary>
    public Vector3? Position { get; set; }

    /// <summary>
    ///     Animated sprite ID, or null if the entity does not have an animated sprite.
    /// </summary>
    public int? AnimatedSpriteId { get; set; }

    /// <summary>
    ///     Set to true if entity should be drawable, false otherwise.
    /// </summary>
    public bool Drawable { get; set; }

    /// <summary>
    ///     Block material information if the entity is a block, or null if not a block.
    /// </summary>
    public MaterialPair? Material { get; set; }

    /// <summary>
    ///     Set to true if entity is a player character, false otherwise.
    /// </summary>
    public bool PlayerCharacter { get; set; }

    /// <summary>
    ///     Name of the entity, or null if the entity is not named.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Parent entity ID, or null if the entity has no parent.
    /// </summary>
    public ulong? Parent { get; set; }
}