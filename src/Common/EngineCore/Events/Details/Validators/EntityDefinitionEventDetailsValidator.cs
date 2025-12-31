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
using Microsoft.Extensions.Logging;

namespace Sovereign.EngineCore.Events.Details.Validators;

public class EntityDefinitionEventDetailsValidator : IEventDetailsValidator
{
    /// <summary>
    ///     Maximum definitions per message.
    /// </summary>
    private const int MAX_DEFINITIONS = 256;

    private readonly EntityDefinitionValidator definitionValidator;
    private readonly ILogger<EntityDefinitionEventDetailsValidator> logger;

    public EntityDefinitionEventDetailsValidator(EntityDefinitionValidator definitionValidator,
        ILogger<EntityDefinitionEventDetailsValidator> logger)
    {
        this.definitionValidator = definitionValidator;
        this.logger = logger;
    }

    public bool IsValid(IEventDetails? details)
    {
        if (details == null || details.GetType() != typeof(EntityDefinitionEventDetails)) return false;

        var castDetails = (EntityDefinitionEventDetails)details;
        if (castDetails.EntityDefinitions.Count > MAX_DEFINITIONS)
        {
            logger.LogError("Received event with too many details ({Count}).", castDetails.EntityDefinitions.Count);
            return false;
        }

        return !castDetails.EntityDefinitions
            .Select(def => definitionValidator.Validate(def))
            .Contains(false);
    }
}