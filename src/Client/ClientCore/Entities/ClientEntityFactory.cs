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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity factory for the client.
/// </summary>
public sealed class ClientEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AdminTagCollection admins;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly EntityAssigner assigner;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly DrawableTagCollection drawables;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly KinematicComponentCollection kinematics;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly PlayerCharacterTagCollection playerCharacterTags;

    public ClientEntityFactory(EntityManager entityManager,
        KinematicComponentCollection kinematics,
        DrawableTagCollection drawables,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        AnimatedSpriteComponentCollection animatedSprites,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents,
        OrientationComponentCollection orientations,
        AdminTagCollection admins,
        BlockPositionComponentCollection blockPositions,
        EntityTable entityTable)
    {
        this.entityManager = entityManager;
        this.kinematics = kinematics;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;
        this.orientations = orientations;
        this.admins = admins;
        this.blockPositions = blockPositions;
        this.entityTable = entityTable;

        assigner = entityManager.GetNewAssigner();
    }

    public IEntityBuilder GetBuilder(bool isNewTemplate = false)
    {
        return GetBuilder(assigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId, bool isLoad = false)
    {
        return new ClientEntityBuilder(entityId, isLoad,
            entityManager, kinematics, drawables, materials,
            materialModifiers, aboveBlocks, animatedSprites, playerCharacterTags, names, parents,
            orientations, admins, blockPositions, entityTable);
    }
}