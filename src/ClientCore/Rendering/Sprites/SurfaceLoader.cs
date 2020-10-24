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

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Display;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for loading SDL_Surfaces and converting them to
    /// the display format.
    /// </summary>
    public class SurfaceLoader
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        public SurfaceLoader(MainDisplay mainDisplay)
        {
            this.mainDisplay = mainDisplay;
        }

        /// <summary>
        /// Loads an image from a file into an SDL_Surface with the same format as
        /// the main display.
        /// </summary>
        /// <param name="filename">Path to the file to load.</param>
        /// <returns>SDL_Surface with the same format as the main display.</returns>
        public Surface LoadSurface(string filename)
        {
            /* Attempt to load the surface. */
            using (var surface = LoadSurfaceFromFile(filename))
            {
                /* Convert the surface to the display format. */
                return ConvertSurfaceToDisplayFormat(surface);
            }
        }

        /// <summary>
        /// Loads a surface from a file in the format of the file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Surface LoadSurfaceFromFile(string filename)
        {
            var surface = new Surface(SDL_image.IMG_Load(filename));
            if (!surface.IsValid)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }
            return surface;
        }
        
        /// <summary>
        /// Converts a surface to the display format.
        /// </summary>
        /// <param name="original">Original surface.</param>
        /// <returns>Surface converted to the display format.</returns>
        private Surface ConvertSurfaceToDisplayFormat(Surface original)
        {
            var targetFormat = mainDisplay.DisplayMode.DisplayFormat;
            var converted = new Surface(original, targetFormat);
            if (!converted.IsValid)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }
            return converted;
        }

    }

}
