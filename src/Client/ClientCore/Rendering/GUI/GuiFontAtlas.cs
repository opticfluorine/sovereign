/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SDL2;
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering.GUI
{

    /// <summary>
    /// Manages the GUI font atlas as an SDL surface.
    /// </summary>
    public sealed class GuiFontAtlas : IDisposable
    {
        /// <summary>
        /// Pointer to the SDL surface containing the font atlas.
        /// </summary>
        private Surface fontAtlasSurface;

        /// <summary>
        /// Width.
        /// </summary>
        private int width;

        /// <summary>
        /// Height.
        /// </summary>
        private int height;

        /// <summary>
        /// Gets a pointer to the SDL surface containing the font atlas.
        /// </summary>
        public Surface FontAtlasSurface
        {
            get
            {
                if (fontAtlasSurface == null)
                {
                    InitializeSurface();
                }

                return fontAtlasSurface;
            }
        }

        /// <summary>
        /// Width of the font atlas, in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                if (fontAtlasSurface == null)
                {
                    InitializeSurface();
                }

                return width;
            }
        }

        /// <summary>
        /// Height of the font atlas, in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                if (fontAtlasSurface == null)
                {
                    InitializeSurface();
                }

                return height;
            }
        }

        public void Dispose()
        {
            fontAtlasSurface?.Dispose();
        }

        /// <summary>
        /// Initializes the font atlas surface.
        /// </summary>
        private void InitializeSurface()
        {
            // Retrieve raw data from ImGui.
            ImGui.GetIO().Fonts.GetTexDataAsRGBA32(out IntPtr outPixels,
                out var outWidth, out var outHeight);
            width = outWidth;
            height = outHeight;

            // Create an SDL_Surface to hold the atlas.
            fontAtlasSurface = Surface.CreateSurfaceFrom(outPixels, outWidth, outHeight,
                DisplayFormat.B8G8R8A8_UNorm);
        }

    }

}
