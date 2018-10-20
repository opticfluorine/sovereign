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
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Manages the tile sprites.
    /// </summary>
    public sealed class TileSpriteManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Tile sprite definitions filename.
        /// </summary>
        public const string TileSpriteDefinitionsFilename = "TileSpriteDefinitions.yaml";

        /// <summary>
        /// List of tile sprites.
        /// </summary>
        public IList<TileSprite> TileSprites { get; private set; } = new List<TileSprite>();

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder pathBuilder;

        /// <summary>
        /// Tile sprite definitions loader.
        /// </summary>
        private readonly TileSpriteDefinitionsLoader loader;

        public TileSpriteManager(IResourcePathBuilder pathBuilder,
            TileSpriteDefinitionsLoader loader)
        {
            this.pathBuilder = pathBuilder;
            this.loader = loader;
        }

        /// <summary>
        /// Initializes the tile sprites.
        /// </summary>
        public void InitializeTileSprites()
        {
            var definitions = LoadDefinitions();
            UnpackDefinitions(definitions);
        }

        /// <summary>
        /// Loads the tile sprite definitions.
        /// </summary>
        /// <returns>Tile sprite definitions.</returns>
        /// <exception cref="FatalErrorException">
        /// Thrown if the tile sprite definitions cannot be loaded.
        /// </exception>
        private TileSpriteDefinitions LoadDefinitions()
        {
            /* Try to load the tile sprite definitions. */
            TileSpriteDefinitions definitions;
            try
            {
                definitions = loader.LoadDefinitions(pathBuilder
                    .BuildPathToResource(ResourceType.Sprite,
                    TileSpriteDefinitionsFilename));
            }
            catch (Exception e)
            {
                /* Log and throw a fatal error. */
                var msg = "Failed to load the tile sprite definitions.";
                Logger.Fatal(msg, e);

                ErrorHandler.Error("Failed to load the tile sprite definitions.\n"
                    + "Refer to the error log for details.");

                throw new FatalErrorException();
            }

            return definitions;
        }

        /// <summary>
        /// Unpacks the tile sprite definitions to populate the list of tile sprites.
        /// </summary>
        /// <param name="definitions">Tile sprite definitions.</param>
        private void UnpackDefinitions(TileSpriteDefinitions definitions)
        {
            foreach (var definition in definitions.TileSprites.OrderBy(tile => tile.Id))
            {
                TileSprites.Add(new TileSprite(definition));
            }
        }

    }

}
