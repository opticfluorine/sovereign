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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Player.Components;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity factory for the client.
/// </summary>
public sealed class ClientEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly EntityAssigner assigner;
    private readonly DrawableTagCollection drawables;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly ParentComponentCollection parents;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;

    public ClientEntityFactory(EntityManager entityManager,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities,
        DrawableTagCollection drawables,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        AnimatedSpriteComponentCollection animatedSprites,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        ParentComponentCollection parents,
        EntityTable entityTable)
    {
        this.entityManager = entityManager;
        this.positions = positions;
        this.velocities = velocities;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.parents = parents;
        this.entityTable = entityTable;

        assigner = entityManager.GetNewAssigner();
    }

    public IEntityBuilder GetBuilder()
    {
        return GetBuilder(assigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId, bool isLoad = false)
    {
        return new ClientEntityBuilder(entityId, isLoad,
            entityManager, positions, velocities, drawables, materials,
            materialModifiers, aboveBlocks, animatedSprites, playerCharacterTags, names, parents, entityTable);
    }
}