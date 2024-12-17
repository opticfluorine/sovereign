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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Threading;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Base builder class for new entities.
/// </summary>
public abstract class AbstractEntityBuilder : IEntityBuilder
{
    protected readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AdminTagCollection admins;
    protected readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly CastBlockShadowsTagCollection castBlockShadows;
    protected readonly DrawableTagCollection drawables;

    protected readonly ulong entityId;
    protected readonly EntityTable entityTable;
    protected readonly bool isTemplate;
    protected readonly KinematicsComponentCollection Kinematics;
    protected readonly bool load;
    protected readonly MaterialModifierComponentCollection materialModifiers;
    protected readonly MaterialComponentCollection materials;
    protected readonly NameComponentCollection names;
    protected readonly OrientationComponentCollection orientations;
    protected readonly ParentComponentCollection parents;
    protected readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PointLightSourceComponentCollection pointLightSources;

    private readonly IncrementalGuard.IncrementalGuardWeakLock weakLock;
    private bool isBlock;
    private bool isDisposed;

    private ulong templateEntityId;

    protected AbstractEntityBuilder(ulong entityId, bool load,
        EntityManager entityManager, KinematicsComponentCollection kinematics,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents,
        DrawableTagCollection drawables,
        AnimatedSpriteComponentCollection animatedSprites,
        OrientationComponentCollection orientations,
        AdminTagCollection admins,
        BlockPositionComponentCollection blockPositions,
        CastBlockShadowsTagCollection castBlockShadows,
        PointLightSourceComponentCollection pointLightSources,
        EntityTable entityTable)
    {
        this.entityId = entityId;
        this.load = load;
        Kinematics = kinematics;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;
        this.drawables = drawables;
        this.animatedSprites = animatedSprites;
        this.orientations = orientations;
        this.entityTable = entityTable;
        this.admins = admins;
        this.blockPositions = blockPositions;
        this.castBlockShadows = castBlockShadows;
        this.pointLightSources = pointLightSources;

        if (entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId)
        {
            entityTable.TakeTemplateEntityId(entityId);
            isTemplate = true;
        }

        weakLock = entityManager.UpdateGuard.AcquireWeakLock();
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            weakLock.Dispose();
            isDisposed = true;
        }
    }

    public ulong Build()
    {
        if (!entityTable.Exists(entityId)) entityTable.Add(entityId, templateEntityId, isBlock, load);
        Dispose();

        return entityId;
    }

    public IEntityBuilder Template(ulong templateEntityId)
    {
        this.templateEntityId = templateEntityId;
        return this;
    }

    public IEntityBuilder Positionable(Kinematics kinematics)
    {
        // Disallowed for template entities.
        if (isTemplate) return this;

        Kinematics.AddOrUpdateComponent(entityId, kinematics, load);
        return this;
    }

    public IEntityBuilder Positionable(Vector3 position, Vector3 velocity)
    {
        return Positionable(new Kinematics { Position = position, Velocity = velocity });
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
        Kinematics.RemoveComponent(entityId, load);
        return this;
    }

    public IEntityBuilder BlockPositionable(GridPosition position)
    {
        // Disallowed for template entities.
        if (isTemplate) return this;

        blockPositions.AddOrUpdateComponent(entityId, position, load);
        isBlock = true;
        return this;
    }

    public IEntityBuilder WithoutBlockPositionable()
    {
        blockPositions.RemoveComponent(entityId, load);
        isBlock = false;
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
        // Disallowed for template entities.
        if (isTemplate) return this;

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
        // Disallowed for template entities.
        if (isTemplate) return this;

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
        // Disallowed for template entities.
        if (isTemplate) return this;

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

    public IEntityBuilder Orientation(Orientation orientation)
    {
        orientations.AddOrUpdateComponent(entityId, orientation, load);
        return this;
    }

    public IEntityBuilder WithoutOrientation()
    {
        orientations.RemoveComponent(entityId, load);
        return this;
    }

    public abstract IEntityBuilder Account(Guid accountId);

    public abstract IEntityBuilder WithoutAccount();

    public IEntityBuilder Admin()
    {
        // Disallowed for template entities.
        if (isTemplate) return this;

        admins.TagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder WithoutAdmin()
    {
        admins.UntagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder CastBlockShadows()
    {
        castBlockShadows.TagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder WithoutCastBlockShadows()
    {
        castBlockShadows.UntagEntity(entityId, load);
        return this;
    }

    public IEntityBuilder PointLightSource(PointLight pointLight)
    {
        pointLightSources.AddComponent(entityId, pointLight, load);
        return this;
    }

    public IEntityBuilder WithoutPointLightSource()
    {
        pointLightSources.RemoveComponent(entityId, load);
        return this;
    }
}