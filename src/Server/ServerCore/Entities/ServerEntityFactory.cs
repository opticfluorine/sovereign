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
public sealed class ServerEntityFactory(
    EntityManager entityManager,
    KinematicsComponentCollection kinematics,
    BlockTileComponentCollection blockTiles,
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
    StackableTagCollection stackables,
    QuantityComponentCollection quantities,
    ItemUseComponentCollection itemUses,
    NpcFlagsComponentCollection npcFlags,
    UseRangeComponentCollection useRanges,
    HealthComponentCollection healths,
    StaminaComponentCollection staminas,
    ManaComponentCollection manas,
    EntityTable entityTable,
    EntityAssigner entityAssigner)
    : IEntityFactory
{
    private ulong nextBlockEntityId = EntityConstants.FirstBlockEntityId;

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
            blockTiles,
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
            stackables,
            quantities,
            itemUses,
            npcFlags,
            useRanges,
            healths,
            staminas,
            manas,
            entityTable);
    }
}
