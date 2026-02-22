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
               AreComponentsValid(definition);
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
}