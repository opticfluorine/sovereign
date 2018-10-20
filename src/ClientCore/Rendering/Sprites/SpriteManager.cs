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
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Manages the sprites.
    /// </summary>
    public sealed class SpriteManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Name of the sprite definitions file.
        /// </summary>
        private const string SpriteDefinitionsFile = "SpriteDefinitions.yaml";

        /// <summary>
        /// Sprite definitions loader.
        /// </summary>
        private readonly SpriteDefinitionsLoader loader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder resourcePathBuilder;

        /// <summary>
        /// Sprites.
        /// </summary>
        public IList<Sprite> Sprites { get; private set; }

        public SpriteManager(SpriteDefinitionsLoader loader,
            IResourcePathBuilder resourcePathBuilder)
        {
            this.loader = loader;
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Initializes the sprites.
        /// </summary>
        public void InitializeSprites()
        {
            var definitions = LoadDefinitions();
            Sprites = UnpackSprites(definitions);
        }

        /// <summary>
        /// Loads the sprite definitions.
        /// </summary>
        /// <returns>Sprite definitions.</returns>
        /// <exception cref="FatalErrorException">
        /// Thrown if the sprite definitions cannot be loaded.
        /// </exception>
        private SpriteDefinitions LoadDefinitions()
        {
            var definitionsPath = resourcePathBuilder.BuildPathToResource(ResourceType.Sprite,
                SpriteDefinitionsFile);

            try
            {
                return loader.LoadSpriteDefinitions(definitionsPath);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to load the sprite definitions.", e);
                ErrorHandler.Error("Failed to load the sprite definitions.\n"
                    + "Refer to the error log for details.");
                throw new FatalErrorException();
            }
        }

        /// <summary>
        /// Unpacks the sprite definitions.
        /// </summary>
        /// <param name="definitions">Sprite definitions.</param>
        /// <returns>Sprites.</returns>
        private IList<Sprite> UnpackSprites(SpriteDefinitions definitions)
        {
            return definitions.Sprites
                .OrderBy(sprite => sprite.Id)
                .ToList();
        }

    }

}
