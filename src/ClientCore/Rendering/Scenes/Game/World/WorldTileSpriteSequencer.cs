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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.World.Materials;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public WorldTileSpriteSequencer(MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            BlockGridPositionIndexer blockGridPositions,
            MaterialManager materialManager,
            AboveBlockComponentCollection aboveBlocks,
            TileSpriteManager tileSpriteManager)
        {
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.blockGridPositions = blockGridPositions;
            this.materialManager = materialManager;
            this.aboveBlocks = aboveBlocks;
            this.tileSpriteManager = tileSpriteManager;
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
                /* Obtain grid coordinates for self and neighbors. */
                var center = (GridPosition)tileSpriteInfo.Position;
                var north = center + GridPosition.OneY;
                var south = center - GridPosition.OneY;
                var east = center + GridPosition.OneX;
                var west = center - GridPosition.OneX;

                /* Get tile sprites (or wildcards) for neighbors. */
                var centerId = tileSpriteInfo.Id;
                var northId = GetTileSpriteIdAtPosition(north, isTopFace);
                var southId = GetTileSpriteIdAtPosition(south, isTopFace);
                var eastId = GetTileSpriteIdAtPosition(east, isTopFace);
                var westId = GetTileSpriteIdAtPosition(west, isTopFace);

                /* Resolve the tile sprite. */
                var tileSprite = tileSpriteManager.TileSprites[centerId];
                var resolvedSprites = tileSprite.GetMatchingAnimatedSpriteIds(northId, eastId,
                    southId, westId);
                foreach (var sprite in resolvedSprites)
                {
                    /* Sequence sprite. */
                    animatedSprites.Add(new PosVelId()
                    {
                        Id = sprite,
                        Position = tileSpriteInfo.Position,
                        Velocity = tileSpriteInfo.Velocity
                    });
                }
            }
        }

        /// <summary>
        /// Gets the tile sprite ID at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int GetTileSpriteIdAtPosition(GridPosition position, bool isTopFace)
        {
            /* Get the block, or return a wildcard if not found. */
            var blockIds = blockGridPositions.GetEntitiesAtPosition(position);
            if (blockIds.Count == 0) return TileSprite.Wildcard;
            var blockId = blockIds.First();

            /* Get the material information, or return wildcard if not found. */
            var materialId = materials.GetComponentForEntity(blockId);
            var modifier = materialModifiers.GetComponentForEntity(blockId);
            if (!materialId.HasValue || !modifier.HasValue) return TileSprite.Wildcard;

            /* Retrieve the tile sprite information. */
            try
            {
                var material = materialManager.Materials[materialId.Value];
                var subtype = material.MaterialSubtypes[modifier.Value];

                if (isTopFace)
                {
                    var obscured = aboveBlocks.HasComponentForEntity(blockId);
                    return obscured ? subtype.ObscuredTopFaceTileSpriteId
                        : subtype.TopFaceTileSpriteId;
                }
                else
                {
                    return subtype.SideFaceTileSpriteId;
                }
            }
            catch
            {
                return TileSprite.Wildcard;
            }
        }

    }

}
