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
using System.Text;

namespace Sovereign.WorldLib.Materials
{

    /// <summary>
    /// Describes a subtype of a material.
    /// </summary>
    public class MaterialSubtype
    {

        /// <summary>
        /// The material modifier value. Unique within a material.
        /// </summary>
        public int MaterialModifier { get; set; }

        /// <summary>
        /// The ID of the tile sprite used for the top face.
        /// </summary>
        public int TopFaceTileSpriteId { get; set; }

        /// <summary>
        /// The ID of the tile sprite used for the top face if a face is obscured.
        /// </summary>
        public int ObscuredTopFaceTileSpriteId { get; set; }

        /// <summary>
        /// The ID of the tile sprite used for the side face.
        /// </summary>
        public int SideFaceTileSpriteId { get; set; }

    }

}
