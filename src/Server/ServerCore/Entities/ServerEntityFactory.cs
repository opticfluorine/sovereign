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

using System.Threading;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.ServerCore.Components;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Server-side entity factory.
/// </summary>
public sealed class ServerEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AccountComponentCollection accounts;
    private readonly AdminTagCollection admins;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly BoundingBoxComponentCollection boundingBoxes;
    private readonly CastBlockShadowsTagCollection castBlockShadows;
    private readonly CastShadowsComponentCollection castShadows;
    private readonly DrawableComponentCollection drawables;

    private readonly EntityAssigner entityAssigner;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly KinematicsComponentCollection kinematics;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly PhysicsTagCollection physics;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PointLightSourceComponentCollection pointLightSources;
    private readonly ServerOnlyTagCollection serverOnly;

    private ulong nextBlockEntityId = EntityConstants.FirstBlockEntityId;

    public ServerEntityFactory(
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
        ServerOnlyTagCollection serverOnly,
        EntityTable entityTable)
    {
        this.entityManager = entityManager;
        this.kinematics = kinematics;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.accounts = accounts;
        this.parents = parents;
        this.drawables = drawables;
        this.animatedSprites = animatedSprites;
        this.orientations = orientations;
        this.admins = admins;
        this.blockPositions = blockPositions;
        this.castBlockShadows = castBlockShadows;
        this.pointLightSources = pointLightSources;
        this.physics = physics;
        this.boundingBoxes = boundingBoxes;
        this.castShadows = castShadows;
        this.entityTypes = entityTypes;
        this.serverOnly = serverOnly;
        this.entityTable = entityTable;
        entityAssigner = entityManager.GetNewAssigner();
    }

    public IEntityBuilder GetBuilder(EntityCategory entityCategory, bool load = false)
    {
        return entityCategory switch
        {
            EntityCategory.Block => GetBuilder(Interlocked.Increment(ref nextBlockEntityId), load),
            EntityCategory.Template => GetBuilder(entityTable.TakeNextTemplateEntityId(), load),
            _ => GetBuilder(entityAssigner.GetNextId(), load)
        };
    }

    public IEntityBuilder GetBuilder(ulong entityId, bool load = false)
    {
        return new ServerEntityBuilder(
            entityId,
            load,
            entityManager,
            kinematics,
            materials,
            materialModifiers,
            aboveBlocks,
            playerCharacterTags,
            names,
            accounts,
            parents,
            drawables,
            animatedSprites,
            orientations,
            admins,
            blockPositions,
            castBlockShadows,
            pointLightSources,
            physics,
            boundingBoxes,
            castShadows,
            entityTypes,
            serverOnly,
            entityTable);
    }
}