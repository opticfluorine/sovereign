﻿/*
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
using Sovereign.ClientCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas
{

    /// <summary>
    /// Maps spritesheet coordinates into the texture atlas.
    /// </summary>
    public sealed class AtlasMap
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private readonly TextureAtlasManager atlasManager;
        private readonly SpriteManager spriteManager;
        private readonly SpriteSheetManager spriteSheetManager;
        private readonly IClientConfiguration clientConfiguration;

        /// <summary>
        /// Maps each sprite ID to its texture atlas coordinates.
        /// </summary>
        public IList<AtlasMapElement> MapElements { get; private set; }

        public AtlasMap(TextureAtlasManager atlasManager, SpriteManager spriteManager,
            SpriteSheetManager spriteSheetManager,
            IClientConfiguration clientConfiguration)
        {
            this.atlasManager = atlasManager;
            this.spriteManager = spriteManager;
            this.spriteSheetManager = spriteSheetManager;
            this.clientConfiguration = clientConfiguration;
        }

        /// <summary>
        /// Initializes the atlas map.
        /// </summary>
        /// 
        /// This must be called after the texture atlas and sprite manager are
        /// initialized.
        public void InitializeAtlasMap()
        {
            MapElements = new List<AtlasMapElement>(spriteManager.Sprites.Count);

            /* Iterate in order of sprite ID. */
            foreach (var sprite in spriteManager.Sprites)
            {
                AddSprite(sprite);
            }

            Logger.Info("Mapped " + MapElements.Count + " sprites to the texture atlas.");
        }

        /// <summary>
        /// Adds the given sprite to the map.
        /// </summary>
        /// <param name="sprite">Sprite to be added.</param>
        private void AddSprite(Sprite sprite)
        {
            /* Retrieve spritesheet. */
            var sheet = spriteSheetManager.SpriteSheets[sprite.SpritesheetName];
            var spriteWidth = sheet.Definition.SpriteWidth;
            var spriteHeight = sheet.Definition.SpriteHeight;

            /* Locate spritesheet in atlas. */
            var (stlx, stly) = atlasManager.TextureAtlas.SpriteSheetMap[sprite.SpritesheetName];
            var atlasWidth = atlasManager.TextureAtlas.Width;
            var atlasHeight = atlasManager.TextureAtlas.Height;

            /* Compute sprite coordinates in atlas. */
            var tlx = (float)(stlx + sprite.Column * spriteWidth);
            var tly = (float)(stly + sprite.Row * spriteHeight);
            var brx = tlx + spriteWidth;
            var bry = tly + spriteHeight;

            /* Add record to map. */
            MapElements.Add(new AtlasMapElement()
            {
                NormalizedLeftX = tlx / atlasWidth,
                NormalizedTopY = tly / atlasHeight,
                NormalizedRightX = brx / atlasWidth,
                NormalizedBottomY = bry / atlasHeight,
                WidthInTiles = spriteWidth / clientConfiguration.TileWidth,
                HeightInTiles = spriteHeight / clientConfiguration.TileWidth
            });
        }

    }

}
