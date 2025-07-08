/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.ServerCore.Components;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Server-side entity builder.
/// </summary>
public sealed class ServerEntityBuilder : AbstractEntityBuilder
{
    private readonly AccountComponentCollection accounts;

    public ServerEntityBuilder(ulong entityId, bool load,
        EntityManager entityManager,
        KinematicsComponentCollection kinematics,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        AccountComponentCollection accounts,
        ParentComponentCollection parents,
        DrawableComponentCollection drawables,
        AnimatedSpriteComponentCollection animatedSprites,
        OrientationComponentCollection orientations,
        AdminTagCollection admins,
        BlockPositionComponentCollection blockPositions,
        CastBlockShadowsTagCollection castBlockShadows,
        PointLightSourceComponentCollection pointLightSources,
        PhysicsTagCollection physics,
        BoundingBoxComponentCollection boundingBoxes,
        CastShadowsComponentCollection castShadows,
        EntityTypeComponentCollection entityTypes,
        EntityTable entityTable)
        : base(entityId, load, entityManager, kinematics, materials,
            materialModifiers, aboveBlocks, playerCharacterTags, names, parents,
            drawables, animatedSprites, orientations, admins, blockPositions, castBlockShadows, pointLightSources,
            physics, boundingBoxes, castShadows, entityTypes, entityTable)
    {
        this.accounts = accounts;
    }

    public override IEntityBuilder Account(Guid accountId)
    {
        // Disallowed for template entities.
        if (isTemplate) return this;

        accounts.AddComponent(entityId, accountId, load);
        return this;
    }

    public override IEntityBuilder WithoutAccount()
    {
        accounts.RemoveComponent(entityId, load);
        return this;
    }
}