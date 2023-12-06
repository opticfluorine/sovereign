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

using System;
using System.Numerics;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Utility interface for creating new entities.
/// </summary>
public interface IEntityBuilder
{
    /// <summary>
    ///     Builds the entity.
    /// </summary>
    /// <returns>Entity ID.</returns>
    ulong Build();

    /// <summary>
    ///     Makes the new entity positionable with the given position and velocity.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="velocity">Velocity.</param>
    /// <returns></returns>
    IEntityBuilder Positionable(Vector3 position, Vector3 velocity);

    /// <summary>
    ///     Makes the new entity positionable with the given position and
    ///     zero velocity.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Positionable(Vector3 position);

    /// <summary>
    ///     Makes the new entity positionable with zero position
    ///     and zero velocity.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder Positionable();

    /// <summary>
    ///     Makes the new entity drawable.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder Drawable();

    /// <summary>
    ///     Uses the given animated sprite when rendering the new entity.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder AnimatedSprite(int animatedSpriteId);

    /// <summary>
    ///     Makes the new entity a block of the given material.
    /// </summary>
    /// <param name="materialId">Material ID.</param>
    /// <param name="materialModifier">Material modifier.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Material(int materialId, int materialModifier);

    /// <summary>
    ///     Records the entity ID of the block above this block.
    ///     Note that this does not move either block, only records the
    ///     topological relationship.
    /// </summary>
    /// <param name="aboveBlock"></param>
    /// <returns></returns>
    IEntityBuilder AboveBlock(ulong aboveBlock);

    /// <summary>
    ///     Tags the entity as a player character.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder PlayerCharacter();

    /// <summary>
    ///     Assigns a name to the entity.
    /// </summary>
    /// <param name="name">Entity name.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Name(string name);

    /// <summary>
    ///     Assigns an account ID to the entity.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Account(Guid accountId);

    /// <summary>
    ///     Assigns a parent entity ID to the entity.
    /// </summary>
    /// <param name="parentEntityId">Parent entity ID.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Parent(ulong parentEntityId);
}