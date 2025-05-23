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

using System;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity builder for the client.
/// </summary>
public sealed class ClientEntityBuilder : AbstractEntityBuilder
{
    public ClientEntityBuilder(ulong entityId, bool isLoad,
        EntityManager entityManager,
        KinematicsComponentCollection kinematics,
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
        CastBlockShadowsTagCollection castBlockShadows,
        PointLightSourceComponentCollection pointLightSources,
        PhysicsTagCollection physics,
        BoundingBoxComponentCollection boundingBoxes,
        CastShadowsComponentCollection castShadows,
        EntityTable entityTable)
        : base(entityId, isLoad, entityManager, kinematics, materials,
            materialModifiers, aboveBlocks, playerCharacterTags, names, parents, drawables, animatedSprites,
            orientations, admins, blockPositions, castBlockShadows, pointLightSources, physics, boundingBoxes,
            castShadows, entityTable)
    {
    }

    public override IEntityBuilder Account(Guid accountId)
    {
        /* no-op */
        return this;
    }

    public override IEntityBuilder WithoutAccount()
    {
        // no-op
        return this;
    }
}