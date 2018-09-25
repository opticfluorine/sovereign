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
using Sovereign.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for creating spritesheets from files.
    /// </summary>
    public class SpriteSheetFactory
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Surface loader.
        /// </summary>
        private readonly SurfaceLoader surfaceLoader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder resourcePathBuilder;

        public SpriteSheetFactory(SurfaceLoader surfaceLoader, 
            IResourcePathBuilder resourcePathBuilder)
        {
            this.surfaceLoader = surfaceLoader;
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Loads the spritesheet described by the given definition.
        /// </summary>
        /// <param name="definition">Spritesheet definition.</param>
        /// <returns>Spritesheet.</returns>
        /// <exception cref="SurfaceException">Thrown if the spritesheet file cannot be loaded.</exception>
        public SpriteSheet LoadSpriteSheet(SpriteSheetDefinition definition)
        {
            Logger.DebugFormat("Loading spritesheet {0}: {1}", definition.SheetId,
                definition.Filename);

            /* Attempt to load the spritesheet surface. */
            var surface = LoadSurfaceForSpritesheet(definition);

            /* Bundle the spritesheet. */
            return new SpriteSheet(surface, definition);
        }

        /// <summary>
        /// Loads the spritesheet image file into a Surface.
        /// </summary>
        /// <param name="definition">Spritesheet definition.</param>
        /// <returns>Spritesheet surface.</returns>
        /// <exception cref="SurfaceException">
        /// Thrown if the surface cannot be loaded.
        /// </exception>
        private Surface LoadSurfaceForSpritesheet(SpriteSheetDefinition definition)
        {
            var filepath = resourcePathBuilder.BuildPathToResource(ResourceType.Spritesheet,
                definition.Filename);
            return surfaceLoader.LoadSurface(filepath);
        }

    }

}
