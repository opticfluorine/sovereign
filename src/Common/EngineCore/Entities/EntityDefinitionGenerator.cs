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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Generates EntityDefinition objects for entities.
/// </summary>
public class EntityDefinitionGenerator
{
    private readonly AdminTagCollection admins;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly CastBlockShadowsTagCollection castBlockShadows;
    private readonly DrawableTagCollection drawables;
    private readonly EntityTable entityTable;
    private readonly KinematicsComponentCollection kinematics;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly PlayerCharacterTagCollection playerCharacters;
    private readonly PointLightSourceComponentCollection pointLightSources;

    public EntityDefinitionGenerator(
        KinematicsComponentCollection kinematics,
        MaterialComponentCollection materials, MaterialModifierComponentCollection materialModifiers,
        PlayerCharacterTagCollection playerCharacters, NameComponentCollection names,
        ParentComponentCollection parents, DrawableTagCollection drawables,
        AnimatedSpriteComponentCollection animatedSprites, OrientationComponentCollection orientations,
        AdminTagCollection admins, BlockPositionComponentCollection blockPositions,
        CastBlockShadowsTagCollection castBlockShadows, PointLightSourceComponentCollection pointLightSources,
        EntityTable entityTable)
    {
        this.kinematics = kinematics;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.playerCharacters = playerCharacters;
        this.names = names;
        this.parents = parents;
        this.drawables = drawables;
        this.animatedSprites = animatedSprites;
        this.orientations = orientations;
        this.admins = admins;
        this.blockPositions = blockPositions;
        this.castBlockShadows = castBlockShadows;
        this.pointLightSources = pointLightSources;
        this.entityTable = entityTable;
    }

    /// <summary>
    ///     Generates an entity definition for a single entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Definition.</returns>
    public EntityDefinition GenerateDefinition(ulong entityId)
    {
        var def = new EntityDefinition();
        def.EntityId = entityId;

        def.TemplateEntityId = entityTable.TryGetTemplate(entityId, out var templateEntityId)
            ? templateEntityId
            : 0;

        if (kinematics.HasComponentForEntity(entityId))
            def.Position = kinematics[entityId].Position;

        def.Drawable = drawables.HasTagForEntity(entityId);

        if (animatedSprites.HasComponentForEntity(entityId))
            def.AnimatedSpriteId = animatedSprites[entityId];

        if (materials.HasComponentForEntity(entityId))
            def.Material = new MaterialPair(materials[entityId], materialModifiers[entityId]);

        def.PlayerCharacter = playerCharacters.HasTagForEntity(entityId);

        if (names.HasComponentForEntity(entityId))
            def.Name = names[entityId];

        if (parents.HasComponentForEntity(entityId))
            def.Parent = parents[entityId];

        if (orientations.HasComponentForEntity(entityId))
            def.Orientation = orientations[entityId];

        def.Admin = admins.HasTagForEntity(entityId);

        if (blockPositions.HasComponentForEntity(entityId))
            def.BlockPosition = blockPositions[entityId];

        def.CastBlockShadows = castBlockShadows.HasTagForEntity(entityId);

        if (pointLightSources.HasComponentForEntity(entityId))
            def.PointLightSource = pointLightSources[entityId];

        return def;
    }
}