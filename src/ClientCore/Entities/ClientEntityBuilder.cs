/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;

namespace Sovereign.ClientCore.Entities
{

    /// <summary>
    /// Entity builder for the client.
    /// </summary>
    public sealed class ClientEntityBuilder : AbstractEntityBuilder
    {
        private readonly DrawableComponentCollection drawables;
        private readonly AnimatedSpriteComponentCollection animatedSprites;

        public ClientEntityBuilder(ulong entityId,
            ComponentManager componentManager,
            PositionComponentCollection positions,
            VelocityComponentCollection velocities,
            DrawableComponentCollection drawables,
            MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AboveBlockComponentCollection aboveBlocks,
            AnimatedSpriteComponentCollection animatedSprites)
            : base(entityId, componentManager, positions, velocities, materials, 
                  materialModifiers, aboveBlocks)
        {
            this.drawables = drawables;
            this.animatedSprites = animatedSprites;
        }

        public override IEntityBuilder Drawable()
        {
            drawables.AddComponent(entityId, true);
            return this;
        }

        public override IEntityBuilder AnimatedSprite(int animatedSpriteId)
        {
            animatedSprites.AddComponent(entityId, animatedSpriteId);
            return this;
        }

    }

}
