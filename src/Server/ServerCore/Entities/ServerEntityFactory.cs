/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;
using Sovereign.ServerCore.Components;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Server-side entity factory.
/// </summary>
public sealed class ServerEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AccountComponentCollection accounts;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly DrawableTagCollection drawables;

    private readonly EntityAssigner entityAssigner;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly ParentComponentCollection parents;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;

    public ServerEntityFactory(
        EntityManager entityManager,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        AccountComponentCollection accounts,
        ParentComponentCollection parents,
        DrawableTagCollection drawables,
        AnimatedSpriteComponentCollection animatedSprites,
        EntityTable entityTable)
    {
        this.entityManager = entityManager;
        this.positions = positions;
        this.velocities = velocities;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.accounts = accounts;
        this.parents = parents;
        this.drawables = drawables;
        this.animatedSprites = animatedSprites;
        this.entityTable = entityTable;
        entityAssigner = entityManager.GetNewAssigner();
    }

    public IEntityBuilder GetBuilder()
    {
        return GetBuilder(entityAssigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId, bool load = false)
    {
        return new ServerEntityBuilder(
            entityId,
            load,
            entityManager,
            positions,
            velocities,
            materials,
            materialModifiers,
            aboveBlocks,
            playerCharacterTags,
            names,
            accounts,
            parents,
            drawables,
            animatedSprites,
            entityTable);
    }
}