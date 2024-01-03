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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events.Details.Validators;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Processes entity definitions from EntityDefinitionEventDetails, validating
///     the definitions and constructing entities from valid definitions.
/// </summary>
public class EntityDefinitionProcessor
{
    private readonly IEntityFactory factory;
    private readonly EntityDefinitionValidator validator;

    public EntityDefinitionProcessor(EntityDefinitionValidator validator, IEntityFactory factory)
    {
        this.validator = validator;
        this.factory = factory;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Processes an entity definition to create an entity.
    /// </summary>
    /// <param name="definition"></param>
    public void ProcessDefinition(EntityDefinition definition)
    {
        if (!validator.Validate(definition))
        {
            Logger.WarnFormat("Invalid entity definition for entity ID {0}.", definition.EntityId);
            return;
        }

        // TODO Handle update case if entity already exists.

        // Build entity.
        var builder = factory.GetBuilder(definition.EntityId);
        if (definition.Position.HasValue) builder.Positionable(definition.Position.Value);
        if (definition.AnimatedSpriteId.HasValue) builder.AnimatedSprite(definition.AnimatedSpriteId.Value);
        if (definition.Drawable) builder.Drawable();
        if (definition.Material != null) builder.Material(definition.Material);
        if (definition.PlayerCharacter) builder.PlayerCharacter();
        if (definition.Name != null) builder.Name(definition.Name);
        if (definition.Parent.HasValue) builder.Parent(definition.Parent.Value);
        var entityId = builder.Build();

        Logger.DebugFormat("Built entity ID {0}.", entityId);
    }
}