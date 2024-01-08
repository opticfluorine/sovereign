// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Linq;

namespace Sovereign.EngineCore.Events.Details.Validators;

public class EntityDefinitionEventDetailsValidator : IEventDetailsValidator
{
    /// <summary>
    ///     Maximum definitions per message.
    /// </summary>
    private const int MAX_DEFINITIONS = 256;

    private readonly EntityDefinitionValidator definitionValidator;

    public EntityDefinitionEventDetailsValidator(EntityDefinitionValidator definitionValidator)
    {
        this.definitionValidator = definitionValidator;
    }

    public bool IsValid(IEventDetails? details)
    {
        if (details == null || details.GetType() != typeof(EntityDefinitionEventDetails)) return false;

        var castDetails = (EntityDefinitionEventDetails)details;
        return castDetails.EntityDefinitions.Count <= MAX_DEFINITIONS
               && !castDetails.EntityDefinitions
                   .Select(def => definitionValidator.Validate(def))
                   .Contains(false);
    }
}