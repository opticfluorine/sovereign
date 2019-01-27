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

using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.World.Materials;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Responsible for sequencing tile sprites to animated sprites during
    /// world rendering.
    /// </summary>
    /// <remarks>
    /// The sequencing is done in-place on the sprite lists. The layer's tile
    /// sprite fields will contain animated sprite IDs following sequencing.
    /// </remarks>
    public sealed class WorldTileSpriteSequencer
    {
        private readonly MaterialComponentCollection materials;
        private readonly MaterialModifierComponentCollection materialModifiers;
        private readonly BlockGridPositionIndexer blockGridPositions;
        private readonly MaterialManager materialManager;
        private readonly AboveBlockComponentCollection aboveBlocks;
        private readonly TileSpriteManager tileSpriteManager;
        private readonly IBlockAnimatedSpriteCache spriteCache;

        public WorldTileSpriteSequencer(MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            BlockGridPositionIndexer blockGridPositions,
            MaterialManager materialManager,
            AboveBlockComponentCollection aboveBlocks,
            TileSpriteManager tileSpriteManager,
            IBlockAnimatedSpriteCache spriteCache)
        {
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.blockGridPositions = blockGridPositions;
            this.materialManager = materialManager;
            this.aboveBlocks = aboveBlocks;
            this.tileSpriteManager = tileSpriteManager;
            this.spriteCache = spriteCache;
        }

        /// <summary>
        /// Converts tile sprites to animated sprites and adds them to a render list.
        /// </summary>
        /// <param name="animatedSprites">Animated sprites to render.</param>
        /// <param name="tileSprites">Tile sprites to transform.</param>
        /// <param name="isTopFace">Whether the tile sprites are top faces of blocks.</param>
        public void SequenceTileSprites(IList<PosVelId> animatedSprites,
            IList<PosVelId> tileSprites, bool isTopFace)
        {
            foreach (var tileSpriteInfo in tileSprites)
            {
                /* Resolve animated sprite information from the cache. */
                var cachedSprites = isTopFace
                    ? spriteCache.GetTopFaceAnimatedSpriteIds(tileSpriteInfo.EntityId)
                    : spriteCache.GetFrontFaceAnimatedSpriteIds(tileSpriteInfo.EntityId);

                /* Sequence the sprite information. */
                foreach (var spriteId in cachedSprites)
                {
                    animatedSprites.Add(new PosVelId()
                    {
                        Position = tileSpriteInfo.Position,
                        Velocity = tileSpriteInfo.Velocity,
                        Id = spriteId,
                        EntityId = tileSpriteInfo.EntityId
                    });
                }
            }
        }

    }

}
