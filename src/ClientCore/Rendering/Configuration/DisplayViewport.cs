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

using Sovereign.ClientCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Configures the display viewport used for rendering.
    /// </summary>
    public sealed class DisplayViewport
    {
        private readonly IClientConfiguration clientConfiguration;

        public DisplayViewport(IClientConfiguration clientConfiguration)
        {
            this.clientConfiguration = clientConfiguration;

            WidthInTiles = (float)Width / clientConfiguration.TileWidth;
            HeightInTiles = (float)Height / clientConfiguration.TileWidth;
        }

        /// <summary>
        /// Width of the display viewport in pixels.
        /// </summary>
        public int Width => 800;

        /// <summary>
        /// Height of the display viewport in pixels.
        /// </summary>
        public int Height => 450;

        /// <summary>
        /// Width of the display viewport as a multiple of the tile width.
        /// </summary>
        public float WidthInTiles { get; private set; }

        /// <summary>
        /// Height of the display viewport as a multiple of the tile height.
        /// </summary>
        public float HeightInTiles { get; private set; }

    }

}
