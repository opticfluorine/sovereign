/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
}