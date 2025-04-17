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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Enumeration of all component types.
/// </summary>
public enum ComponentType
{
    #region Common

    /// <summary>
    ///     Position component.
    /// </summary>
    Kinematic = 0x0000,

    /// <summary>
    ///     Material component.
    /// </summary>
    Material = 0x0002,

    /// <summary>
    ///     Material modifier component.
    /// </summary>
    MaterialModifier = 0x0003,

    /// <summary>
    ///     Above block component.
    /// </summary>
    AboveBlock = 0x0004,

    /// <summary>
    ///     Tag indicating that an entity is a player character.
    /// </summary>
    PlayerCharacter = 0x0005,

    /// <summary>
    ///     Component giving an entity its name.
    /// </summary>
    Name = 0x0006,

    /// <summary>
    ///     Component mapping an entity to its parent entity.
    /// </summary>
    Parent = 0x0007,

    /// <summary>
    ///     Animated sprite component.
    /// </summary>
    AnimatedSprite = 0x0008,

    /// <summary>
    ///     Drawable component.
    /// </summary>
    Drawable = 0x0009,

    /// <summary>
    ///     Orientation component.
    /// </summary>
    Orientation = 0x000A,

    /// <summary>
    ///     Admin tag.
    /// </summary>
    Admin = 0x000B,

    /// <summary>
    ///     Block position component.
    /// </summary>
    BlockPosition = 0x000C,

    /// <summary>
    ///     CastBlockShadows tag.
    /// </summary>
    CastBlockShadows = 0x000D,

    /// <summary>
    ///     PointLightSource component.
    /// </summary>
    PointLightSource = 0x000E,

    /// <summary>
    ///     Physics component.
    /// </summary>
    Physics = 0x000F,

    /// <summary>
    ///     BoundingBox component.
    /// </summary>
    BoundingBox = 0x0010,

    /// <summary>
    ///     CastShadows component.
    /// </summary>
    CastShadows = 0x0011,

    #endregion Common

    #region Client

    /// <summary>
    ///     Animation phase component.
    /// </summary>
    AnimationPhase = 0x1000,

    #endregion Client

    #region Server

    /// <summary>
    ///     Account component.
    /// </summary>
    /// <seealso cref="Sovereign.ServerCore.Components.AccountComponentCollection" />
    Account = 0x2000,

    #endregion Server
}