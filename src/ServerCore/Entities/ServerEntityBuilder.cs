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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;

namespace Sovereign.ServerCore.Entities
{

    /// <summary>
    /// Server-side entity builder.
    /// </summary>
    public sealed class ServerEntityBuilder : AbstractEntityBuilder
    {

        public ServerEntityBuilder(ulong entityId,
            ComponentManager componentManager,
            PositionComponentCollection positions,
            VelocityComponentCollection velocities,
            MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AboveBlockComponentCollection aboveBlocks)
            : base(entityId, componentManager, positions, velocities, materials,
                  materialModifiers, aboveBlocks)
        {

        }

        public override IEntityBuilder AnimatedSprite(int animatedSpriteId)
        {
            /* no-op */
            return this;
        }

        public override IEntityBuilder Drawable()
        {
            /* no-op */
            return this;
        }

    }

}
