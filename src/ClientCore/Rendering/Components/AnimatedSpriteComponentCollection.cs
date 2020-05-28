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

namespace Sovereign.ClientCore.Rendering.Components
{

    /// <summary>
    /// The AnimatedSprite component denotes the animated sprite ID to use for
    /// rendering general (non-block) drawable entities.
    /// </summary>
    public sealed class AnimatedSpriteComponentCollection : BaseComponentCollection<int>
    {

        /// <summary>
        /// Initial number of components.
        /// </summary>
        public const int InitialSize = 65536;

        public AnimatedSpriteComponentCollection(ComponentManager manager)
            : base(manager, InitialSize, ComponentOperators.IntOperators,
                  ComponentType.AnimatedSprite)
        {
        }

    }

}
