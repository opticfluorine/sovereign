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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Describes a sprite sheet.
    /// </summary>
    public class SpriteSheetDefinition
    {

        /// <summary>
        /// Name of the spritesheet image file.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Sprite width.
        /// </summary>
        public int SpriteWidth { get; set; }

        /// <summary>
        /// Sprite height.
        /// </summary>
        public int SpriteHeight { get; set; }

        /// <summary>
        /// Author of the spritesheet.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// License rights of the spritesheet.f
        /// </summary>
        public string License { get; set; }

    }

}
