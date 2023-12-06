﻿/*
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

using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity factory for the client.
/// </summary>
public sealed class ClientEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly ComponentManager componentManager;
    private readonly DrawableComponentCollection drawables;
    private readonly EntityManager entityManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly ParentComponentCollection parents;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;
    private EntityAssigner assigner;

    public ClientEntityFactory(EntityManager entityManager,
        ComponentManager componentManager,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities,
        DrawableComponentCollection drawables,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        AnimatedSpriteComponentCollection animatedSprites,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents)
    {
        this.entityManager = entityManager;
        this.componentManager = componentManager;
        this.positions = positions;
        this.velocities = velocities;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;
    }

    public IEntityBuilder GetBuilder()
    {
        if (assigner == null)
            assigner = entityManager.GetNewAssigner();

        return GetBuilder(assigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId)
    {
        return new ClientEntityBuilder(entityId,
            componentManager, positions, velocities, drawables, materials,
            materialModifiers, aboveBlocks, animatedSprites, playerCharacterTags, names, parents);
    }
}