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
        NameComponentCollection names)
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

    public abstract IEntityBuilder Drawable();

    public abstract IEntityBuilder AnimatedSprite(int animatedSpriteId);

    public abstract IEntityBuilder Account(Guid accountId);
}