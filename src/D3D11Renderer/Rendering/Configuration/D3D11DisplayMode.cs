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

using Sovereign.ClientCore.Rendering.Configuration;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Configuration
{

    /// <summary>
    /// Display mode available through Direct3D 11.
    /// </summary>
    public class D3D11DisplayMode : IDisplayMode
    {

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public DisplayFormat DisplayFormat { get; internal set; }

        /// <summary>
        /// DXGI mode description.
        /// </summary>
        internal ModeDescription InternalModeDescription { get; set; }

    }

}
