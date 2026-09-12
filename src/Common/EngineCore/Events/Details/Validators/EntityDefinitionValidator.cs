// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Events.Details.Validators;

/// <summary>
///     Validates EntityDefinitionEventDetails objects.
/// </summary>
public class EntityDefinitionValidator(
    NameComponentValidator nameComponentValidator,
    ShadowComponentValidator shadowComponentValidator,
    EntityTypeComponentValidator entityTypeComponentValidator,
    ILogger<EntityDefinitionValidator> logger)
{
    private readonly ILogger<EntityDefinitionValidator> logger = logger;

    /// <summary>
    ///     Validates a single entity definition.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool Validate(EntityDefinition definition)
    {
        return IsNotPositionedChildEntity(definition) &&
               IsCompleteIfPlayerCharacter(definition) &&
               IsNotDoublePositioned(definition) &&
               AreComponentsValid(definition) &&
               IsUseRangeValid(definition) &&
               IsHealthValid(definition) &&
               IsStaminaValid(definition) &&
               IsManaValid(definition) &&
               IsStatsValid(definition) &&
               IsEquipmentTypeValid(definition);
    }

    /// <summary>
    ///     Determines if the components are valid.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool AreComponentsValid(EntityDefinition definition)
    {
        var valid = definition.TemplateEntityId is 0 or >= EntityConstants.FirstTemplateEntityId
            and <= EntityConstants.LastTemplateEntityId;

        if (definition.Name != null)
            valid = valid && nameComponentValidator.IsValid(definition.Name);

        if (definition.CastShadows != null)
            valid = valid && shadowComponentValidator.IsValid(definition.CastShadows);

        valid = valid && entityTypeComponentValidator.IsValid(definition.EntityType);

        if (!valid)
            logger.LogError("Definition for {Id:X} has invalid components.", definition.EntityId);

        return valid;
    }

    /// <summary>
    ///     Checks that if the entity is positioned, it does not have a parent entity.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if definition is valid for this rule, false otherwise.</returns>
    private bool IsNotPositionedChildEntity(EntityDefinition definition)
    {
        var result = definition is not { Position: not null, Parent: not null }
                     && definition is not { BlockPosition: not null, Parent: not null };

        if (!result) logger.LogError("Definition for {Id:X} is a positioned child entity.", definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the UseRange component, if present, is only applied to items with a non-negative range.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsUseRangeValid(EntityDefinition definition)
    {
        if (!definition.UseRange.HasValue) return true;

        var result = definition.EntityType == EntityType.Item && definition.UseRange.Value >= 0f;
        if (!result)
            logger.LogError(
                "Definition for {Id:X} has an invalid UseRange; it must be non-negative and only items may have it.",
                definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that if the entity is a player character, then it has all required
    ///     components specified.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsCompleteIfPlayerCharacter(EntityDefinition definition)
    {
        var result = !definition.PlayerCharacter ||
                     definition is
                     {
                         Position: not null,
                         Name: not null
                     };

        if (!result) logger.LogError("Definition for {Id:X} is an incomplete player character.", definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the entity does not have both a position and a block position.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsNotDoublePositioned(EntityDefinition definition)
    {
        var result = definition is not { Position: not null, BlockPosition: not null };
        if (!result) logger.LogError("Definition for {Id:X} is dual-positioned.", definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the Health component, if present, has a non-negative maximum value.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsHealthValid(EntityDefinition definition)
    {
        if (definition.Health is not { } health) return true;

        var result = health.MaxValue >= 0;
        if (!result)
            logger.LogError("Definition for {Id:X} has an invalid Health; MaxValue must be non-negative.",
                definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the Stamina component, if present, has a non-negative maximum value.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsStaminaValid(EntityDefinition definition)
    {
        if (definition.Stamina is not { } stamina) return true;

        var result = stamina.MaxValue >= 0;
        if (!result)
            logger.LogError("Definition for {Id:X} has an invalid Stamina; MaxValue must be non-negative.",
                definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the Mana component, if present, has a non-negative maximum value.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsManaValid(EntityDefinition definition)
    {
        if (definition.Mana is not { } mana) return true;

        var result = mana.MaxValue >= 0;
        if (!result)
            logger.LogError("Definition for {Id:X} has an invalid Mana; MaxValue must be non-negative.",
                definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the Stats component, if present, is only applied to players and NPCs.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsStatsValid(EntityDefinition definition)
    {
        if (!definition.Stats.HasValue) return true;

        var result = definition.EntityType is EntityType.Player or EntityType.Npc;
        if (!result)
            logger.LogError(
                "Definition for {Id:X} has an invalid Stats; only players and NPCs may have it.",
                definition.EntityId);
        return result;
    }

    /// <summary>
    ///     Checks that the EquipmentType component, if present, is only applied to items and
    ///     equipment slots with a valid value, and that equipment slots always specify one.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsEquipmentTypeValid(EntityDefinition definition)
    {
        if (definition.EntityType == EntityType.EquipmentSlot && !definition.EquipmentType.HasValue)
        {
            logger.LogError(
                "Definition for {Id:X} is an equipment slot without an EquipmentType.",
                definition.EntityId);
            return false;
        }

        if (!definition.EquipmentType.HasValue) return true;

        var result = (int)definition.EquipmentType.Value >= 0 &&
                     (int)definition.EquipmentType.Value < EquipmentConstants.EquipmentSlotCount &&
                     definition.EntityType is EntityType.Item or EntityType.EquipmentSlot;
        if (!result)
            logger.LogError(
                "Definition for {Id:X} has an invalid EquipmentType; it must be a valid equipment type and only items and equipment slots may have it.",
                definition.EntityId);
        return result;
    }
}
