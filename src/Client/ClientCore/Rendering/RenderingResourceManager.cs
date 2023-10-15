/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.World.Materials;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Responsible for managing resources used by rendering.
/// </summary>
public class RenderingResourceManager
{
    private readonly AnimatedSpriteManager animatedSpriteManager;

    private readonly AtlasMap atlasMap;

    private readonly MaterialManager materialManager;

    private readonly RenderingMaterialManager renderingMaterialManager;

    private readonly SpriteManager spriteManager;

    private readonly SpriteSheetManager spriteSheetManager;

    private readonly TextureAtlasManager textureAtlasManager;

    private readonly TileSpriteManager tileSpriteManager;

    public RenderingResourceManager(SpriteSheetManager spriteSheetManager,
        TextureAtlasManager textureAtlasManager,
        TileSpriteManager tileSpriteManager,
        AnimatedSpriteManager animatedSpriteManager,
        SpriteManager spriteManager,
        AtlasMap atlasMap,
        MaterialManager materialManager,
        RenderingMaterialManager renderingMaterialManager)
    {
        this.spriteSheetManager = spriteSheetManager;
        this.textureAtlasManager = textureAtlasManager;
        this.tileSpriteManager = tileSpriteManager;
        this.animatedSpriteManager = animatedSpriteManager;
        this.spriteManager = spriteManager;
        this.atlasMap = atlasMap;
        this.materialManager = materialManager;
        this.renderingMaterialManager = renderingMaterialManager;
    }

    /// <summary>
    ///     Loads and initializes the resources used by rendering.
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
        atlasMap.InitializeAtlasMap();

        /* Initialize materials. */
        materialManager.InitializeMaterials();
        renderingMaterialManager.InitializeRenderingMaterials();
    }

    /// <summary>
    ///     Cleans up all resources used by rendering.
    /// </summary>
    public void CleanupResources()
    {
        textureAtlasManager.ReleaseTextureAtlas();
        spriteSheetManager.ReleaseSpriteSheets();
    }
}