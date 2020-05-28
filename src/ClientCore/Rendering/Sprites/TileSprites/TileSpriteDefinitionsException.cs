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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Exception type thrown when errors related to tilesprite definitions occur.
    /// </summary>
    public sealed class TileSpriteDefinitionsException : ApplicationException
    {
        public TileSpriteDefinitionsException()
        {
        }

        public TileSpriteDefinitionsException(string message) : base(message)
        {
        }

        public TileSpriteDefinitionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
