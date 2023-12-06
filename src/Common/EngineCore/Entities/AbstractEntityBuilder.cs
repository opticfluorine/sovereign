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
    protected readonly ComponentManager componentManager;

    protected readonly ulong entityId;
    protected readonly MaterialModifierComponentCollection materialModifiers;
    protected readonly MaterialComponentCollection materials;
    protected readonly NameComponentCollection names;
    protected readonly ParentComponentCollection parents;
    protected readonly PlayerCharacterTagCollection playerCharacterTags;
    protected readonly PositionComponentCollection positions;
    protected readonly VelocityComponentCollection velocities;

    private IncrementalGuard.IncrementalGuardWeakLock weakLock;

    public AbstractEntityBuilder(ulong entityId,
        ComponentManager componentManager, PositionComponentCollection positions,
        VelocityComponentCollection velocities, MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents)
    {
        this.entityId = entityId;
        this.componentManager = componentManager;
        this.positions = positions;
        this.velocities = velocities;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;

        weakLock = componentManager.ComponentGuard.AcquireWeakLock();
    }

    public void Dispose()
    {
        weakLock?.Dispose();
    }

    public ulong Build()
    {
        weakLock.Dispose();
        weakLock = null;

        return entityId;
    }

    public IEntityBuilder Positionable(Vector3 position, Vector3 velocity)
    {
        positions.AddComponent(entityId, position);
        velocities.AddComponent(entityId, velocity);
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

    public IEntityBuilder Material(int materialId, int materialModifier)
    {
        materials.AddComponent(entityId, materialId);
        materialModifiers.AddComponent(entityId, materialModifier);
        return this;
    }

    public IEntityBuilder AboveBlock(ulong otherEntityId)
    {
        aboveBlocks.AddComponent(entityId, otherEntityId);
        return this;
    }

    public IEntityBuilder PlayerCharacter()
    {
        playerCharacterTags.TagEntity(entityId);
        return this;
    }

    public IEntityBuilder Name(string name)
    {
        names.AddComponent(entityId, name);
        return this;
    }

    public IEntityBuilder Parent(ulong parentEntityId)
    {
        parents.AddComponent(entityId, parentEntityId);
        return this;
    }

    public abstract IEntityBuilder Drawable();

    public abstract IEntityBuilder AnimatedSprite(int animatedSpriteId);

    public abstract IEntityBuilder Account(Guid accountId);
}