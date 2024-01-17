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

using System;
using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity builder for the client.
/// </summary>
public sealed class ClientEntityBuilder : AbstractEntityBuilder
{
    private readonly AnimatedSpriteComponentCollection animatedSprites;

    public ClientEntityBuilder(ulong entityId, bool isLoad,
        EntityManager entityManager,
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
        : base(entityId, isLoad, entityManager, positions, velocities, materials,
            materialModifiers, aboveBlocks, playerCharacterTags, names, parents, drawables, entityTable)
    {
        this.animatedSprites = animatedSprites;
    }

    public override IEntityBuilder AnimatedSprite(int animatedSpriteId)
    {
        animatedSprites.AddComponent(entityId, animatedSpriteId, load);
        return this;
    }

    public override IEntityBuilder WithoutAnimatedSprite()
    {
        drawables.RemoveComponent(entityId, load);
        return this;
    }

    public override IEntityBuilder Account(Guid accountId)
    {
        /* no-op */
        return this;
    }

    public override IEntityBuilder WithoutAccount()
    {
        // no-op
        return this;
    }
}