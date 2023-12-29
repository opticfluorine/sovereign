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

using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Events.Details.Validators;

/// <summary>
///     Validates EntityDefinitionEventDetails objects.
/// </summary>
public class EntityDefinitionValidator
{
    /// <summary>
    ///     Validates a single entity definition.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool Validate(EntityDefinition definition)
    {
        return IsNotPositionedChildEntity(definition) &&
               IsCompleteIfPlayerCharacter(definition);
    }

    /// <summary>
    ///     Checks that if the entity is positioned, it does not have a parent entity.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if definition is valid for this rule, false otherwise.</returns>
    private bool IsNotPositionedChildEntity(EntityDefinition definition)
    {
        return definition is not { Position: not null, Parent: not null };
    }

    /// <summary>
    ///     Checks that if the entity is a player character, then it has all required
    ///     components specified.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <returns>true if valid for this rule, false otherwise.</returns>
    private bool IsCompleteIfPlayerCharacter(EntityDefinition definition)
    {
        return !definition.PlayerCharacter ||
               definition is
               {
                   Position: not null,
                   Name: not null
               };
    }
}