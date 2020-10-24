/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
