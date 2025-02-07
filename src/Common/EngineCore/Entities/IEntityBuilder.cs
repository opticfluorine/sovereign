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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Utility interface for creating and updating entities.
/// </summary>
public interface IEntityBuilder : IDisposable
{
    /// <summary>
    ///     Builds the entity.
    /// </summary>
    /// <returns>Entity ID.</returns>
    ulong Build();

    /// <summary>
    ///     Assigns a template to the entity.
    /// </summary>
    /// <param name="templateEntityId">Template entity ID.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Template")]
    IEntityBuilder Template(ulong templateEntityId);

    /// <summary>
    ///     Makes the new entity positionable with the given position and velocity.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="velocity">Velocity.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Positionable(Vector3 position, Vector3 velocity);

    /// <summary>
    ///     Makes the new entity positionable with the given kinematics.
    /// </summary>
    /// <param name="kinematics">Kinematics.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Kinematics")]
    IEntityBuilder Positionable(Kinematics kinematics);

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
    ///     Removes positionable components if they are currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutPositionable();

    /// <summary>
    ///     Makes the new entity a block with the given grid position.
    /// </summary>
    /// <param name="position">Grid position.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("BlockPosition")]
    IEntityBuilder BlockPositionable(GridPosition position);

    /// <summary>
    ///     Removes block position components if they are currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutBlockPositionable();

    /// <summary>
    ///     Makes the new entity drawable.
    /// </summary>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Drawable")]
    IEntityBuilder Drawable();

    /// <summary>
    ///     Removes drawable component if they are currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutDrawable();

    /// <summary>
    ///     Uses the given animated sprite when rendering the new entity.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("AnimatedSprite")]
    IEntityBuilder AnimatedSprite(int animatedSpriteId);

    /// <summary>
    ///     Removes animated sprite component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutAnimatedSprite();

    /// <summary>
    ///     Makes the new entity a block of the given material.
    /// </summary>
    /// <param name="materialId">Material ID.</param>
    /// <param name="materialModifier">Material modifier.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Material(int materialId, int materialModifier);

    /// <summary>
    ///     Makes the new entity a block of the given material.
    /// </summary>
    /// <param name="material">Material.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Material(MaterialPair material);

    /// <summary>
    ///     Removes material components if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutMaterial();

    /// <summary>
    ///     Records the entity ID of the block above this block.
    ///     Note that this does not move either block, only records the
    ///     topological relationship.
    /// </summary>
    /// <param name="aboveBlock">Above block.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder AboveBlock(ulong aboveBlock);

    /// <summary>
    ///     Removes the above block component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutAboveBlock();

    /// <summary>
    ///     Tags the entity as a player character.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder PlayerCharacter();

    /// <summary>
    ///     Removes the player component tag if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutPlayerCharacter();

    /// <summary>
    ///     Assigns a name to the entity.
    /// </summary>
    /// <param name="name">Entity name.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Name")]
    IEntityBuilder Name(string name);

    /// <summary>
    ///     Removes the name component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutName();

    /// <summary>
    ///     Assigns an account ID to the entity.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Builder.</returns>
    IEntityBuilder Account(Guid accountId);

    /// <summary>
    ///     Removes the account component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutAccount();

    /// <summary>
    ///     Assigns a parent entity ID to the entity.
    /// </summary>
    /// <param name="parentEntityId">Parent entity ID.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Parent")]
    IEntityBuilder Parent(ulong parentEntityId);

    /// <summary>
    ///     Removes the parent component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutParent();

    /// <summary>
    ///     Assigns an orientation to the entity.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Orientation")]
    IEntityBuilder Orientation(Orientation orientation);

    /// <summary>
    ///     Removes the orientation component if currently set.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutOrientation();

    /// <summary>
    ///     Tags the entity as an admin.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder Admin();

    /// <summary>
    ///     Untags the entity as an admin.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutAdmin();

    /// <summary>
    ///     Tags the entity as casting block shadows.
    /// </summary>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("CastBlockShadows")]
    IEntityBuilder CastBlockShadows();

    /// <summary>
    ///     Untags the entity as casting block shadows.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutCastBlockShadows();

    /// <summary>
    ///     Adds a PointLightSource component to the entity.
    /// </summary>
    /// <param name="pointLight">Point light source details.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("PointLightSource")]
    IEntityBuilder PointLightSource(PointLight pointLight);

    /// <summary>
    ///     Removes the PointLightSource component if present.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutPointLightSource();

    /// <summary>
    ///     Tags the entity for physics processing.
    /// </summary>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("Physics")]
    IEntityBuilder Physics();

    /// <summary>
    ///     Removes the Physics tag if present.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutPhysics();

    /// <summary>
    ///     Adds a BoundingBox component to the entity.
    /// </summary>
    /// <param name="boundingBox">Bounding box.</param>
    /// <returns>Builder.</returns>
    [ScriptableEntityBuilderAction("BoundingBox")]
    IEntityBuilder BoundingBox(BoundingBox boundingBox);

    /// <summary>
    ///     Removes the BoundingBox component if present.
    /// </summary>
    /// <returns>Builder.</returns>
    IEntityBuilder WithoutBoundingBox();
}