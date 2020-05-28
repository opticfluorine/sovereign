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

            Logger.Info("Loaded " + AnimatedSprites.Count + " animated sprites.");
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
                Logger.Fatal("Failed to load the animated sprite definitions.", e);
                ErrorHandler.Error("Failed to load the animated sprite definitions.\n"
                    + "Refer to the error log for details.");

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
