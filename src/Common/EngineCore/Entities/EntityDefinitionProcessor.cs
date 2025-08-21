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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events.Details.Validators;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Processes entity definitions from EntityDefinitionEventDetails, validating
///     the definitions and constructing entities from valid definitions.
/// </summary>
public class EntityDefinitionProcessor
{
    private readonly IEntityFactory factory;
    private readonly ILogger<EntityDefinitionProcessor> logger;
    private readonly EntityDefinitionValidator validator;

    public EntityDefinitionProcessor(EntityDefinitionValidator validator, IEntityFactory factory,
        ILogger<EntityDefinitionProcessor> logger)
    {
        this.validator = validator;
        this.factory = factory;
        this.logger = logger;
    }

    /// <summary>
    ///     Processes an entity definition to create an entity.
    /// </summary>
    /// <param name="definition"></param>
    public void ProcessDefinition(EntityDefinition definition)
    {
        if (!validator.Validate(definition))
        {
            logger.LogWarning("Invalid entity definition for entity ID {Id:X}.", definition.EntityId);
            return;
        }

        // Build entity.
        var builder = factory.GetBuilder(definition.EntityId);
        builder.Template(definition.TemplateEntityId);

        if (definition.Position.HasValue)
            builder.Positionable(definition.Position.Value);
        else
            builder.WithoutPositionable();

        if (definition.AnimatedSpriteId.HasValue)
            builder.AnimatedSprite(definition.AnimatedSpriteId.Value);
        else
            builder.WithoutAnimatedSprite();

        if (definition.Drawable.HasValue)
            builder.Drawable(definition.Drawable.Value);
        else
            builder.WithoutDrawable();

        if (definition.Material != null)
            builder.Material(definition.Material);
        else
            builder.WithoutMaterial();

        if (definition.PlayerCharacter)
            builder.PlayerCharacter();
        else
            builder.WithoutPlayerCharacter();

        if (definition.Name != null)
            builder.Name(definition.Name);
        else
            builder.WithoutName();

        if (definition.Parent.HasValue)
            builder.Parent(definition.Parent.Value);
        else
            builder.WithoutParent();

        if (definition.Orientation.HasValue)
            builder.Orientation(definition.Orientation.Value);
        else
            builder.WithoutOrientation();

        if (definition.Admin)
            builder.Admin();
        else
            builder.WithoutAdmin();

        if (definition.BlockPosition.HasValue)
            builder.BlockPositionable(definition.BlockPosition.Value);
        else
            builder.WithoutBlockPositionable();

        if (definition.CastBlockShadows)
            builder.CastBlockShadows();
        else
            builder.WithoutCastBlockShadows();

        if (definition.PointLightSource.HasValue)
            builder.PointLightSource(definition.PointLightSource.Value);
        else
            builder.WithoutPointLightSource();

        if (definition.Physics)
            builder.Physics();
        else
            builder.WithoutPhysics();

        if (definition.BoundingBox.HasValue)
            builder.BoundingBox(definition.BoundingBox.Value);
        else
            builder.WithoutBoundingBox();

        if (definition.CastShadows.HasValue)
            builder.CastShadows(definition.CastShadows.Value);
        else
            builder.WithoutCastShadows();

        if (definition.EntityType != EntityType.Other)
            builder.EntityType(definition.EntityType);
        else
            builder.WithoutEntityType();

        if (definition.ServerOnly)
            builder.ServerOnly();
        else
            builder.WithoutServerOnly();

        var entityId = builder.Build();
        logger.LogDebug("Processed entity ID {Id:X}.", entityId);
    }
}