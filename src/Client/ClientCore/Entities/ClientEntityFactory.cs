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

using System.Threading;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity factory for the client.
/// </summary>
public sealed class ClientEntityFactory(
    EntityManager entityManager,
    KinematicsComponentCollection kinematics,
    DrawableComponentCollection drawables,
    BlockTileComponentCollection blockTiles,
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
    StatsComponentCollection stats,
    EntityTable entityTable,
    EntityAssigner entityAssigner)
    : IEntityFactory
{
    /// <summary>
    ///     Next available block entity ID.
    /// </summary>
    private ulong nextBlockId = EntityConstants.FirstBlockEntityId;

    public IEntityBuilder GetBuilder(EntityCategory entityCategory, bool load = false)
    {
        return entityCategory switch
        {
            EntityCategory.Block => GetBuilder(Interlocked.Increment(ref nextBlockId), load),
            EntityCategory.Template => GetBuilder(entityTable.TakeNextTemplateEntityId(), load),
            _ => GetBuilder(entityAssigner.GetNextId(), load)
        };
    }

    public IEntityBuilder GetBuilder(ulong entityId, bool isLoad = false)
    {
        return new ClientEntityBuilder(entityId, isLoad,
            entityManager, kinematics, drawables, blockTiles,
            aboveBlocks, animatedSprites, playerCharacterTags, names, parents,
            orientations, admins, blockPositions, castBlockShadows, pointLightSources,
            physics, boundingBoxes, castShadows, entityTypes, serverOnly, stackables, quantities,
            itemUses, npcFlags, useRanges, healths, staminas, manas, stats, entityTable);
    }
}
