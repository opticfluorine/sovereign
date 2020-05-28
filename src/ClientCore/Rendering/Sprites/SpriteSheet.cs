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

using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// A set of sprites loaded from a single file.
    /// </summary>
    public class SpriteSheet : IDisposable
    {

        /// <summary>
        /// Spritesheet definition.
        /// </summary>
        public SpriteSheetDefinition Definition { get; private set; }

        /// <summary>
        /// SDL_Surface holding the spriteset.
        /// </summary>
        public Surface Surface { get; private set; }

        public SpriteSheet(Surface surface, SpriteSheetDefinition definition)
        {
            Surface = surface;
            Definition = definition;
        }

        public void Dispose()
        {
            Surface.Dispose();
        }

    }

}
