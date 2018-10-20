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

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// Manages the animated sprites.
    /// </summary>
    public sealed class AnimatedSpriteManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Resource category used by the animated sprite definitions.
        /// </summary>
        public const ResourceType DefinitionsResourceType = ResourceType.Sprite;

        /// <summary>
        /// Animated sprite definitions filename.
        /// </summary>
        public const string DefinitionsFilename = "AnimatedSpriteDefinitions.yaml";

        /// <summary>
        /// Animated sprites.
        /// </summary>
        public readonly IList<AnimatedSprite> AnimatedSprites = new List<AnimatedSprite>();

        /// <summary>
        /// Animated sprite definitions loader.
        /// </summary>
        private readonly AnimatedSpriteDefinitionsLoader loader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder pathBuilder;

        /// <summary>
        /// Sprite manager.
        /// </summary>
        private readonly SpriteManager spriteManager;

        public AnimatedSpriteManager(AnimatedSpriteDefinitionsLoader loader,
            IResourcePathBuilder pathBuilder, SpriteManager spriteManager)
        {
            this.loader = loader;
            this.pathBuilder = pathBuilder;
            this.spriteManager = spriteManager;
        }

        /// <summary>
        /// Initializes the animated sprites.
        /// </summary>
        public void InitializeAnimatedSprites()
        {
            var definitions = LoadDefinitions();
            UnpackDefinitions(definitions);
        }

        /// <summary>
        /// Loads the animated sprite definitions.
        /// </summary>
        /// <returns>Animated sprite definitions.</returns>
        /// <exception cref="FatalErrorException">
        /// Thrown if the definitions cannot be loaded.
        /// </exception>
        private AnimatedSpriteDefinitions LoadDefinitions()
        {
            var filename = pathBuilder.BuildPathToResource(DefinitionsResourceType, 
                DefinitionsFilename);
            try
            {
                return loader.LoadDefinitions(filename);
            }
            catch (Exception e)
            {
                /* Log and throw a fatal error. */
                Logger.Fatal("Failed to load animated sprite definitions.", e);
                ErrorHandler.Error(e.Message);

                throw new FatalErrorException();
            }
        }

        /// <summary>
        /// Unpacks the animated sprite definitions.
        /// </summary>
        /// <param name="definitions">Definitions to unpack.</param>
        private void UnpackDefinitions(AnimatedSpriteDefinitions definitions)
        {
            foreach (var def in definitions.AnimatedSprites.OrderBy(def => def.Id))
            {
                AnimatedSprites.Add(new AnimatedSprite(def, spriteManager));
            }
        }

    }

}
