/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.World.Materials;
using Sovereign.EngineUtil.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Systems.Block.Caches
{

    /// <summary>
    /// Client-side animated sprite cache for the block system.
    /// </summary>
    /// <remarks>
    /// The cache updates once per tick immediately following the material, material
    /// modifier, position, and above block component updates.  It depends on these in 
    /// order to ensure that any newly added block has both its position and material 
    /// defined.
    /// </remarks>
    public sealed class BlockAnimatedSpriteCache : IBlockAnimatedSpriteCache, IDisposable
    {
        private readonly MaterialComponentCollection materials;
        private readonly MaterialModifierComponentCollection materialModifiers;
        private readonly PositionComponentCollection positions;
        private readonly BlockGridPositionIndexer blockGridPositions;
        private readonly BlockPositionEventFilter blockPositionEventFilter;
        private readonly MaterialManager materialManager;
        private readonly AboveBlockComponentCollection aboveBlocks;
        private readonly TileSpriteManager tileSpriteManager;

        /// <summary>
        /// Block set pool.
        /// </summary>
        private readonly ObjectPool<HashSet<ulong>> blockSetPool
            = new ObjectPool<HashSet<ulong>>(4);

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Number of component collections to expect updates against per tick.
        /// </summary>
        private const int UpdatedCollectionCount = 4;

        /// <summary>
        /// Block entity IDs that have been added or modified since the last cache update.
        /// </summary>
        private HashSet<ulong> changedBlocks;

        /// <summary>
        /// Block entity IDs that have been removed since the last cache update.
        /// </summary>
        private HashSet<ulong> removedBlocks;

        /// <summary>
        /// Top face animated sprite cache.
        /// </summary>
        private readonly IDictionary<ulong, IList<int>> topFaceCache
            = new ConcurrentDictionary<ulong, IList<int>>();

        /// <summary>
        /// Front face animated sprite cache.
        /// </summary>
        private readonly IDictionary<ulong, IList<int>> frontFaceCache
            = new ConcurrentDictionary<ulong, IList<int>>();

        /// <summary>
        /// Internal cache of known current block positions.
        /// </summary>
        private readonly IDictionary<ulong, GridPosition> knownPositions
            = new ConcurrentDictionary<ulong, GridPosition>();

        /// <summary>
        /// Read-only empty list to return when no sprites are cached.
        /// </summary>
        private readonly IList<int> emptyList
            = new ReadOnlyCollection<int>(new List<int>());

        /// <summary>
        /// Number of component update rounds since the last cache update.
        /// </summary>
        private int updateCount = 0;

        public BlockAnimatedSpriteCache(MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            PositionComponentCollection positions,
            BlockGridPositionIndexer blockGridPositions,
            BlockPositionEventFilter blockPositionEventFilter,
            MaterialManager materialManager,
            AboveBlockComponentCollection aboveBlocks,
            TileSpriteManager tileSpriteManager)
        {
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.positions = positions;
            this.blockGridPositions = blockGridPositions;
            this.blockPositionEventFilter = blockPositionEventFilter;
            this.materialManager = materialManager;
            this.aboveBlocks = aboveBlocks;
            this.tileSpriteManager = tileSpriteManager;

            RegisterEventHandlers();
        }

        public void Dispose()
        {
            DeregisterEventHandlers();
        }

        public IList<int> GetFrontFaceAnimatedSpriteIds(ulong blockId)
        {
            return frontFaceCache.TryGetValue(blockId, out var spriteIds) ? spriteIds : emptyList;
        }

        public IList<int> GetTopFaceAnimatedSpriteIds(ulong blockId)
        {
            return topFaceCache.TryGetValue(blockId, out var spriteIds) ? spriteIds : emptyList;
        }

        /// <summary>
        /// Updates the animated sprite cache.
        /// </summary>
        private void UpdateCache(HashSet<ulong> changedSet, HashSet<ulong> removedSet)
        {
            try
            {
                // Update the cache with the changes.
                HandleChangedBlocks(changedSet);
                HandleRemovedBlocks(removedSet);

                // Return the sets to the pool so that they can be reused later.
                blockSetPool.ReturnObject(changedSet);
                blockSetPool.ReturnObject(removedSet);
            }
            catch (Exception e)
            {
                Logger.Error("Error updating sprite cache.", e);
            }
        }

        /// <summary>
        /// Updates the cache for any blocks that have changed.
        /// </summary>
        /// <param name="changedSet">Set of changed blocks.</param>
        private void HandleChangedBlocks(ISet<ulong> changedSet)
        {
            foreach (var blockId in changedSet)
            {
                UpdatePositionForBlock(blockId);

                if (!knownPositions.ContainsKey(blockId))
                {
                    Logger.ErrorFormat("No position known for changed block ID {0}.", blockId);
                    continue;
                }

                UpdateCacheForBlock(blockId, knownPositions[blockId], true);
            }
        }

        /// <summary>
        /// Updates the cahce for any blocks that have been removed.
        /// </summary>
        /// <param name="removedSet">Set of removed blocks.</param>
        private void HandleRemovedBlocks(ISet<ulong> removedSet)
        {
            foreach (var blockId in removedSet)
            {
                RemoveEntity(blockId);

                if (!knownPositions.ContainsKey(blockId))
                {
                    Logger.ErrorFormat("No position known for removed block ID {0}.", blockId);
                    continue;
                }

                UpdateCacheForBlock(blockId, knownPositions[blockId], false);

                knownPositions.Remove(blockId);
            }
        }

        /// <summary>
        /// Updates the cache for a change to a given block.
        /// </summary>
        /// <param name="blockId">Block entity ID that is changed or removed.</param>
        /// <param name="gridPosition">Grid position of the block.</param>
        /// <param name="updateSelf">Whether to update the cache entry for the block itself.</param>
        private void UpdateCacheForBlock(ulong blockId, GridPosition gridPosition, bool updateSelf)
        {
            /* Identify neighbors. */
            var north = gridPosition + GridPosition.OneY;
            var south = gridPosition - GridPosition.OneY;
            var east = gridPosition + GridPosition.OneX;
            var west = gridPosition - GridPosition.OneX;
            var below = gridPosition - GridPosition.OneZ;

            /* Update cache. */
            var isTopFace = true;
            for (int i = 0; i < 2; ++i)
            {
                UpdateCacheAtPosition(north, isTopFace);
                UpdateCacheAtPosition(south, isTopFace);
                UpdateCacheAtPosition(east, isTopFace);
                UpdateCacheAtPosition(west, isTopFace);
                if (isTopFace) UpdateCacheAtPosition(below, true);
                if (updateSelf) UpdateCacheAtPosition(gridPosition, isTopFace);

                isTopFace = !isTopFace;
            }
        }

        /// <summary>
        /// Updates the cache entry for the block at the given position, if any.
        /// </summary>
        /// <param name="gridPosition">Position to update.</param>
        /// <param name="isTopFace">If true, update top face; otherwise update front face.</param>
        private void UpdateCacheAtPosition(GridPosition gridPosition, bool isTopFace)
        {
            /* Confirm that a block is present at the given position. */
            var blockIds = blockGridPositions.GetEntitiesAtPosition(gridPosition);
            if (blockIds == null || blockIds.Count == 0) return;

            /* Resolve to the tile sprite level. */
            var blockId = blockIds.First();
            var centerId = GetTileSpriteIdForBlock(blockId, isTopFace);
            if (centerId == -1)
            {
                // Block isn't ready yet, return at a later pass.
                return;
            }

            /* Resolve neighbors to the tile sprite level. */
            var north = gridPosition + GridPosition.OneY;
            var south = gridPosition - GridPosition.OneY;
            var east = gridPosition + GridPosition.OneX;
            var west = gridPosition - GridPosition.OneX;
            var northId = GetTileSpriteIdForPosition(north, isTopFace);
            var southId = GetTileSpriteIdForPosition(south, isTopFace);
            var eastId = GetTileSpriteIdForPosition(east, isTopFace);
            var westId = GetTileSpriteIdForPosition(west, isTopFace);

            /* Resolve tile sprite to animated sprites. */
            var tileSprite = tileSpriteManager.TileSprites[centerId];
            var resolvedSprites = tileSprite.GetMatchingAnimatedSpriteIds(northId, eastId,
                southId, westId);

            /* Retrieve and populate cache. */
            var cacheList = GetCacheList(blockId, isTopFace);
            cacheList.Clear();
            foreach (var animatedSpriteId in resolvedSprites)
            {
                cacheList.Add(animatedSpriteId);
            }
        }

        /// <summary>
        /// Gets the cache list for the given block face.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <param name="isTopFace">Top face if true; front face otherwise.</param>
        /// <returns>Cache list.</returns>
        private IList<int> GetCacheList(ulong blockId, bool isTopFace)
        {
            var dict = isTopFace ? topFaceCache : frontFaceCache;
            if (!dict.ContainsKey(blockId))
            {
                dict[blockId] = new List<int>();
            }
            return dict[blockId];
        }

        /// <summary>
        /// Gets the tile sprite ID for the block at the given position, if any.
        /// </summary>
        /// <param name="gridPosition">Grid position.</param>
        /// <param name="isTopFace">Whether to resolve the top face.</param>
        /// <returns>Resolved tile sprite ID, or TileSprite.Wildcard if no valid block.</returns>
        private int GetTileSpriteIdForPosition(GridPosition gridPosition, bool isTopFace)
        {
            /* Get block at position, or return wildcard if not found. */
            var blockIds = blockGridPositions.GetEntitiesAtPosition(gridPosition);
            if (blockIds == null || blockIds.Count == 0) return TileSprite.Wildcard;

            return GetTileSpriteIdForBlock(blockIds.First(), isTopFace);
        }

        /// <summary>
        /// Gets the tile sprite ID for the given block.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <param name="isTopFace">If true, resolve the top face; otherwise resolve the front face.</param>
        /// <returns>Resolved tile sprite ID.</returns>
        private int GetTileSpriteIdForBlock(ulong blockId, bool isTopFace)
        {
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


        /// <summary>
        /// Updates the known position for the given block.
        /// </summary>
        /// <param name="blockId">Block to update.</param>
        private void UpdatePositionForBlock(ulong blockId)
        {
            if (!positions.HasComponentForEntity(blockId)) return;

            knownPositions[blockId] = (GridPosition)positions[blockId];
        }

        /// <summary>
        /// Removes an entity from the cache entirely.
        /// </summary>
        /// <param name="blockId">Block entity ID to remove.</param>
        private void RemoveEntity(ulong blockId)
        {
            topFaceCache.Remove(blockId);
            frontFaceCache.Remove(blockId);
        }

        /// <summary>
        /// Called when a materials update begins.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void OnStartUpdates(object sender, EventArgs e)
        {
            if (updateCount == 0)
            {
                // First update pass for this tick, start new collections.
                changedBlocks = blockSetPool.TakeObject();
                removedBlocks = blockSetPool.TakeObject();
                changedBlocks.Clear();
                removedBlocks.Clear();
            }

            updateCount++;
        }

        /// <summary>
        /// Called when a materials update ends.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void OnEndUpdates(object sender, EventArgs e)
        {
            /* Check whether both materials and modifiers have updated. */
            if (updateCount >= UpdatedCollectionCount)
            {
                /* Update the cache asynchronously. */
                if (changedBlocks.Count > 0 || removedBlocks.Count > 0)
                {
                    var changedSet = changedBlocks;
                    var removedSet = removedBlocks;
                    Task.Factory.StartNew(() => UpdateCache(changedSet, removedSet));
                }

                /* Reset state for the next tick. */
                changedBlocks = null;
                removedBlocks = null;
                updateCount = 0;
            }
        }

        /// <summary>
        /// Called when a component is added or modified.
        /// </summary>
        /// <param name="entityId">Block entity ID.</param>
        /// <param name="componentValue">Not used.</param>
        private void OnComponentChanged(ulong entityId, int componentValue)
        {
            changedBlocks.Add(entityId);
        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="entityId">Block entity ID.</param>
        private void OnComponentRemoved(ulong entityId)
        {
            removedBlocks.Add(entityId);
        }

        /// <summary>
        /// Called when a block position is added or modified.
        /// </summary>
        /// <param name="entityId">Block entity ID.</param>
        /// <param name="componentValue">Not used.</param>
        private void OnPositionChanged(ulong entityId, Vector3 componentValue)
        {
            OnComponentChanged(entityId, 0);
        }

        /// <summary>
        /// Registers the event handlers.
        /// </summary>
        private void RegisterEventHandlers()
        {
            materials.OnStartUpdates += OnStartUpdates;
            materials.OnComponentAdded += OnComponentChanged;
            materials.OnComponentModified += OnComponentChanged;
            materials.OnComponentRemoved += OnComponentRemoved;
            materials.OnEndUpdates += OnEndUpdates;

            materialModifiers.OnStartUpdates += OnStartUpdates;
            materialModifiers.OnComponentAdded += OnComponentChanged;
            materialModifiers.OnComponentModified += OnComponentChanged;
            materialModifiers.OnComponentRemoved += OnComponentRemoved;
            materialModifiers.OnEndUpdates += OnEndUpdates;

            positions.OnStartUpdates += OnStartUpdates;
            positions.OnEndUpdates += OnEndUpdates;

            blockPositionEventFilter.OnComponentAdded += OnPositionChanged;
            blockPositionEventFilter.OnComponentModified += OnPositionChanged;

            aboveBlocks.OnStartUpdates += OnStartUpdates;
            aboveBlocks.OnEndUpdates += OnEndUpdates;
        }

        /// <summary>
        /// Deregisters the event handlers.
        /// </summary>
        private void DeregisterEventHandlers()
        {
            materials.OnStartUpdates -= OnStartUpdates;
            materials.OnComponentAdded -= OnComponentChanged;
            materials.OnComponentModified -= OnComponentChanged;
            materials.OnComponentRemoved -= OnComponentRemoved;
            materials.OnEndUpdates -= OnEndUpdates;

            materialModifiers.OnStartUpdates -= OnStartUpdates;
            materialModifiers.OnComponentAdded -= OnComponentChanged;
            materialModifiers.OnComponentModified -= OnComponentChanged;
            materialModifiers.OnComponentRemoved -= OnComponentRemoved;
            materialModifiers.OnEndUpdates -= OnEndUpdates;

            positions.OnStartUpdates -= OnStartUpdates;
            positions.OnEndUpdates -= OnEndUpdates;

            blockPositionEventFilter.OnComponentAdded -= OnPositionChanged;
            blockPositionEventFilter.OnComponentModified -= OnPositionChanged;

            aboveBlocks.OnStartUpdates -= OnStartUpdates;
            aboveBlocks.OnEndUpdates -= OnEndUpdates;
        }

    }

}
