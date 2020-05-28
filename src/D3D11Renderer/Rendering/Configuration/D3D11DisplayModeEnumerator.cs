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
    /// Enumerates the display modes available to an adapter.
    /// </summary>
    public class D3D11DisplayModeEnumerator : IDisplayModeEnumerator
    {

        /// <summary>
        /// Engine display format corresponding to the display format to be used.
        /// </summary>
        private const DisplayFormat DefaultDisplayFormat = DisplayFormat.R8G8B8A8_UNorm;

        public IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter)
        {
            var videoAdapter = (D3D11VideoAdapter)adapter;
            return GetNativeModes(videoAdapter.InternalAdapter)
                .Select(ConvertMode);
        }

        /// <summary>
        /// Gets the D3D11 display modes available to the given adapter.
        /// </summary>
        /// <param name="adapter">D3D11 video adapter.</param>
        /// <returns>Available display modes.</returns>
        private IEnumerable<ModeDescription> GetNativeModes(Adapter adapter)
        {
            return adapter.Outputs.SelectMany(
                output => output.GetDisplayModeList(D3D11RendererConstants.DisplayFormat, 0))
                .Distinct();
        }

        private D3D11DisplayMode ConvertMode(ModeDescription modeDescription)
        {
            var mode = new D3D11DisplayMode
            {
                Width = modeDescription.Width,
                Height = modeDescription.Height,
                DisplayFormat = DefaultDisplayFormat,
                InternalModeDescription = modeDescription,
            };
            return mode;
        }

    }

}
