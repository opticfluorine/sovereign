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

using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.World.Materials;
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

        private readonly SpriteSheetManager spriteSheetManager;

        private readonly TextureAtlasManager textureAtlasManager;

        private readonly TileSpriteManager tileSpriteManager;

        private readonly AnimatedSpriteManager animatedSpriteManager;

        private readonly SpriteManager spriteManager;

        private readonly MaterialManager materialManager;

        private readonly RenderingMaterialManager renderingMaterialManager;

        public RenderingResourceManager(SpriteSheetManager spriteSheetManager,
            TextureAtlasManager textureAtlasManager,
            TileSpriteManager tileSpriteManager,
            AnimatedSpriteManager animatedSpriteManager,
            SpriteManager spriteManager,
            MaterialManager materialManager,
            RenderingMaterialManager renderingMaterialManager)
        {
            this.spriteSheetManager = spriteSheetManager;
            this.textureAtlasManager = textureAtlasManager;
            this.tileSpriteManager = tileSpriteManager;
            this.animatedSpriteManager = animatedSpriteManager;
            this.spriteManager = spriteManager;
            this.materialManager = materialManager;
            this.renderingMaterialManager = renderingMaterialManager;
        }

        /// <summary>
        /// Loads and initializes the resources used by rendering.
        /// </summary>
        public void InitializeResources()
        {
            /* Initialize spritesheets. */
            spriteSheetManager.InitializeSpriteSheets();
            textureAtlasManager.InitializeTextureAtlas();

            /* Initialize sprite abstractions. */
            spriteManager.InitializeSprites();
            animatedSpriteManager.InitializeAnimatedSprites();
            tileSpriteManager.InitializeTileSprites();

            /* Initialize materials. */
            materialManager.InitializeMaterials();
            renderingMaterialManager.InitializeRenderingMaterials();
        }

        /// <summary>
        /// Cleans up all resources used by rendering.
        /// </summary>
        public void CleanupResources()
        {
            textureAtlasManager.ReleaseTextureAtlas();
            spriteSheetManager.ReleaseSpriteSheets();
        }

    }

}
