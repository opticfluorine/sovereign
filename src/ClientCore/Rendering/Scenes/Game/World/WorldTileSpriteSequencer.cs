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
