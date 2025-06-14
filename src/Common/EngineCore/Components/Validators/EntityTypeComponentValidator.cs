// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Components.Validators;

/// <summary>
///     Validator for EntityType components.
/// </summary>
public class EntityTypeComponentValidator
{
    /// <summary>
    ///     Determines if the entity type is valid.
    /// </summary>
    /// <param name="entityType">Entity type.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(EntityType entityType)
    {
        // Check if the entity type is valid
        return entityType == EntityType.Npc ||
               entityType == EntityType.Item ||
               entityType == EntityType.Player;
    }
}