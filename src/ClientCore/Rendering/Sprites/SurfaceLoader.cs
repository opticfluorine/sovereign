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
