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

using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Responsible for managing resources used by rendering.
    /// </summary>
    public class RenderingResourceManager
    {

        /// <summary>
        /// Spritesheet manager.
        /// </summary>
        public SpriteSheetManager SpriteSheetManager { get; private set; }

        /// <summary>
        /// Texture atlas manager.
        /// </summary>
        public TextureAtlasManager TextureAtlasManager { get; private set; }

        /// <summary>
        /// Tile sprite manager.
        /// </summary>
        public TileSpriteManager TileSpriteManager { get; private set; }

        /// <summary>
        /// Animated sprite manager.
        /// </summary>
        public AnimatedSpriteManager AnimatedSpriteManager { get; private set; }

        /// <summary>
        /// Sprite manager.
        /// </summary>
        public SpriteManager SpriteManager { get; private set; }

        public RenderingResourceManager(SpriteSheetManager spriteSheetManager,
            TextureAtlasManager textureAtlasManager,
            TileSpriteManager tileSpriteManager,
            AnimatedSpriteManager animatedSpriteManager,
            SpriteManager spriteManager)
        {
            SpriteSheetManager = spriteSheetManager;
            TextureAtlasManager = textureAtlasManager;
            TileSpriteManager = tileSpriteManager;
            AnimatedSpriteManager = animatedSpriteManager;
            SpriteManager = spriteManager;
        }

        /// <summary>
        /// Loads and initializes the resources used by rendering.
        /// </summary>
        public void InitializeResources()
        {
            /* Initialize spritesheets. */
            SpriteSheetManager.InitializeSpriteSheets();
            TextureAtlasManager.InitializeTextureAtlas();

            /* Initialize sprite abstractions. */
            SpriteManager.InitializeSprites();
            AnimatedSpriteManager.InitializeAnimatedSprites();
            TileSpriteManager.InitializeTileSprites();
        }

        /// <summary>
        /// Cleans up all resources used by rendering.
        /// </summary>
        public void CleanupResources()
        {
            TextureAtlasManager.ReleaseTextureAtlas();
            SpriteSheetManager.ReleaseSpriteSheets();
        }

    }

}
