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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;
using Sovereign.EngineUtil.Threading;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Base builder class for new entities.
/// </summary>
public abstract class AbstractEntityBuilder : IEntityBuilder, IDisposable
{
    protected readonly AboveBlockComponentCollection aboveBlocks;
    protected readonly AnimatedSpriteComponentCollection animatedSprites;
    protected readonly DrawableTagCollection drawables;

    protected readonly ulong entityId;
    protected readonly EntityTable entityTable;
    protected readonly bool load;
    protected readonly MaterialModifierComponentCollection materialModifiers;
    protected readonly MaterialComponentCollection materials;
    protected readonly NameComponentCollection names;
    protected readonly ParentComponentCollection parents;
    protected readonly PlayerCharacterTagCollection playerCharacterTags;
    protected readonly PositionComponentCollection positions;
    protected readonly VelocityComponentCollection velocities;

    private readonly IncrementalGuard.IncrementalGuardWeakLock weakLock;

    protected AbstractEntityBuilder(ulong entityId, bool load,
        EntityManager entityManager, PositionComponentCollection positions,
        VelocityComponentCollection velocities, MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents,
        DrawableTagCollection drawables,
        AnimatedSpriteComponentCollection animatedSprites,
        EntityTable entityTable)
    {
        this.entityId = entityId;
        this.load = load;
        this.positions = positions;
        this.velocities = velocities;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;
        this.drawables = drawables;
        this.animatedSprites = animatedSprites;
        this.entityTable = entityTable;

        weakLock = entityManager.UpdateGuard.AcquireWeakLock();
    }

    public void Dispose()
    {
        weakLock.Dispose();
    }

    public ulong Build()
    {
        if (!entityTable.Exists(entityId)) entityTable.Add(entityId);
        Dispose();

        return entityId;
    }

    public IEntityBuilder Positionable(Vector3 position, Vector3 velocity)
    {
        positions.AddOrUpdateComponent(entityId, position, load);
        velocities.AddOrUpdateComponent(entityId, velocity, load);
        return this;
    }

    public IEntityBuilder Positionable(Vector3 position)
    {
        return Positionable(position, Vector3.Zero);
    }

    public IEntityBuilder Positionable()
    {
        return Positionable(Vector3.Zero, Vector3.Zero);
    }

    public IEntityBuilder WithoutPositionable()
    {
        positions.RemoveComponent(entityId, load);
        velocities.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder Material(int materialId, int materialModifier)
    {
        materials.AddOrUpdateComponent(entityId, materialId, load);
        materialModifiers.AddOrUpdateComponent(entityId, materialModifier, load);
        return this;
    }

    public IEntityBuilder Material(MaterialPair material)
    {
        return Material(material.MaterialId, material.MaterialModifier);
    }

    public IEntityBuilder WithoutMaterial()
    {
        materials.RemoveComponent(entityId, load);
        materialModifiers.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder AboveBlock(ulong otherEntityId)
    {
        aboveBlocks.AddOrUpdateComponent(entityId, otherEntityId, load);
        return this;
    }

    public IEntityBuilder WithoutAboveBlock()
    {
        aboveBlocks.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder PlayerCharacter()
    {
        playerCharacterTags.TagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder WithoutPlayerCharacter()
    {
        playerCharacterTags.UntagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder Name(string name)
    {
        names.AddOrUpdateComponent(entityId, name, load);
        return this;
    }

    public IEntityBuilder WithoutName()
    {
        names.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder Parent(ulong parentEntityId)
    {
        parents.AddOrUpdateComponent(entityId, parentEntityId, load);
        return this;
    }

    public IEntityBuilder WithoutParent()
    {
        parents.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder Drawable()
    {
        drawables.TagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder WithoutDrawable()
    {
        drawables.UntagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder AnimatedSprite(int animatedSpriteId)
    {
        animatedSprites.AddOrUpdateComponent(entityId, animatedSpriteId, load);
        return this;
    }

    public IEntityBuilder WithoutAnimatedSprite()
    {
        animatedSprites.RemoveComponent(entityId, load);
        return this;
    }

    public abstract IEntityBuilder Account(Guid accountId);

    public abstract IEntityBuilder WithoutAccount();
}