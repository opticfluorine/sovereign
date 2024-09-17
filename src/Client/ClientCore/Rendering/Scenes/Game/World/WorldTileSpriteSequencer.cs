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

using System.Collections.Generic;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Responsible for sequencing tile sprites to animated sprites during
///     world rendering.
/// </summary>
/// <remarks>
///     The sequencing is done in-place on the sprite lists. The layer's tile
///     sprite fields will contain animated sprite IDs following sequencing.
/// </remarks>
public sealed class WorldTileSpriteSequencer
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly BlockGridPositionIndexer blockGridPositions;
    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly OrientationComponentCollection orientations;
    private readonly IBlockAnimatedSpriteCache spriteCache;
    private readonly TileSpriteManager tileSpriteManager;

    public WorldTileSpriteSequencer(MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        BlockGridPositionIndexer blockGridPositions,
        MaterialManager materialManager,
        AboveBlockComponentCollection aboveBlocks,
        TileSpriteManager tileSpriteManager,
        IBlockAnimatedSpriteCache spriteCache,
        OrientationComponentCollection orientations)
    {
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.blockGridPositions = blockGridPositions;
        this.materialManager = materialManager;
        this.aboveBlocks = aboveBlocks;
        this.tileSpriteManager = tileSpriteManager;
        this.spriteCache = spriteCache;
        this.orientations = orientations;
    }

    /// <summary>
    ///     Converts tile sprites to animated sprites and adds them to a render list.
    /// </summary>
    /// <param name="animatedSprites">Animated sprites to render.</param>
    /// <param name="tileSprites">Tile sprites to transform.</param>
    /// <param name="isTopFace">Whether the tile sprites are top faces of blocks.</param>
    public void SequenceTileSprites(List<PosVelId> animatedSprites,
        List<PosVelId> tileSprites, bool isTopFace)
    {
        foreach (var tileSpriteInfo in tileSprites)
        {
            /* Resolve animated sprite information from the cache. */
            var cachedSprites = isTopFace
                ? spriteCache.GetTopFaceAnimatedSpriteIds(tileSpriteInfo.EntityId)
                : spriteCache.GetFrontFaceAnimatedSpriteIds(tileSpriteInfo.EntityId);

            /* Sequence the sprite information. */
            foreach (var spriteId in cachedSprites)
                animatedSprites.Add(tileSpriteInfo with
                {
                    Id = spriteId,
                    Orientation = orientations.GetComponentForEntity(tileSpriteInfo.EntityId)
                        .OrElseDefault(Orientation.South)
                });
        }
    }
}