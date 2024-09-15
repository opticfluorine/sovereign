/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.ClientCore.Systems.Block.Caches;

/// <summary>
///     Client-side animated sprite cache for the block system.
/// </summary>
/// <remarks>
///     The cache updates once per tick immediately following the material, material
///     modifier, position, and above block component updates.  It depends on these in
///     order to ensure that any newly added block has both its position and material
///     defined.
/// </remarks>
public sealed class BlockAnimatedSpriteCache : IBlockAnimatedSpriteCache, IDisposable
{
    /// <summary>
    ///     Number of component collections to expect updates against per tick.
    /// </summary>
    private const int UpdatedCollectionCount = 4;

    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly BlockGridPositionIndexer blockIndexer;
    private readonly BlockPositionComponentCollection blockPositions;

    /// <summary>
    ///     Block set pool.
    /// </summary>
    private readonly ObjectPool<HashSet<ulong>> blockSetPool = new(4);

    /// <summary>
    ///     Read-only empty list to return when no sprites are cached.
    /// </summary>
    private readonly List<int> emptyList = new();

    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;

    /// <summary>
    ///     Front face animated sprite cache.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, List<int>> frontFaceCache = new();

    /// <summary>
    ///     Internal cache of known current block positions.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, GridPosition> knownPositions = new();

    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;

    /// <summary>
    ///     Block entity IDs whose templates have changed since the last cache update.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, byte> templateChanges = new();

    private readonly TileSpriteManager tileSpriteManager;

    /// <summary>
    ///     Top face animated sprite cache.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, List<int>> topFaceCache = new();

    /// <summary>
    ///     Block entity IDs that have been added or modified since the last cache update.
    /// </summary>
    private HashSet<ulong> changedBlocks = new();

    /// <summary>
    ///     Flag indicating that all cached entries need to be refreshed (e.g. for a template change).
    /// </summary>
    private bool refreshAll;

    /// <summary>
    ///     Block entity IDs that have been removed since the last cache update.
    /// </summary>
    private HashSet<ulong> removedBlocks = new();

    /// <summary>
    ///     Number of component update rounds since the last cache update.
    /// </summary>
    private int updateCount;

    public BlockAnimatedSpriteCache(MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        BlockPositionComponentCollection blockPositions,
        BlockGridPositionIndexer blockIndexer,
        MaterialManager materialManager,
        AboveBlockComponentCollection aboveBlocks,
        TileSpriteManager tileSpriteManager,
        EntityManager entityManager, EntityTable entityTable)
    {
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.blockPositions = blockPositions;
        this.blockIndexer = blockIndexer;
        this.materialManager = materialManager;
        this.aboveBlocks = aboveBlocks;
        this.tileSpriteManager = tileSpriteManager;
        this.entityManager = entityManager;
        this.entityTable = entityTable;

        RegisterEventHandlers();
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public List<int> GetFrontFaceAnimatedSpriteIds(ulong blockId)
    {
        return frontFaceCache.TryGetValue(blockId, out var spriteIds) ? spriteIds : emptyList;
    }

    public List<int> GetTopFaceAnimatedSpriteIds(ulong blockId)
    {
        return topFaceCache.TryGetValue(blockId, out var spriteIds) ? spriteIds : emptyList;
    }

    public void Dispose()
    {
        DeregisterEventHandlers();
    }

    /// <summary>
    ///     Updates the animated sprite cache.
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
    ///     Refreshes the animated sprite cache.
    /// </summary>
    private void RefreshCache()
    {
        foreach (var pos in knownPositions.Values) UpdateCacheForBlock(pos, true);
    }

    /// <summary>
    ///     Updates the cache for any blocks that have changed.
    /// </summary>
    /// <param name="changedSet">Set of changed blocks.</param>
    private void HandleChangedBlocks(ISet<ulong> changedSet)
    {
        foreach (var blockId in changedSet)
        {
            // Ignore templates.
            if (blockId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId)
                continue;

            UpdatePositionForBlock(blockId);

            if (!knownPositions.TryGetValue(blockId, out var position))
            {
                Logger.ErrorFormat("No position known for changed block ID {0}.", blockId);
                continue;
            }

            UpdateCacheForBlock(position, true);
        }
    }

    /// <summary>
    ///     Updates the cahce for any blocks that have been removed.
    /// </summary>
    /// <param name="removedSet">Set of removed blocks.</param>
    private void HandleRemovedBlocks(ISet<ulong> removedSet)
    {
        foreach (var blockId in removedSet)
        {
            // Ignore templates.
            if (blockId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId)
                continue;

            RemoveEntity(blockId);

            if (!knownPositions.ContainsKey(blockId))
            {
                Logger.ErrorFormat("No position known for removed block ID {0}.", blockId);
                continue;
            }

            UpdateCacheForBlock(knownPositions[blockId], false);

            knownPositions.TryRemove(blockId, out _);
        }
    }

    /// <summary>
    ///     Updates the cache for a change to a given block.
    /// </summary>
    /// <param name="gridPosition">Grid position of the block.</param>
    /// <param name="updateSelf">Whether to update the cache entry for the block itself.</param>
    private void UpdateCacheForBlock(GridPosition gridPosition, bool updateSelf)
    {
        /* Update cache. */
        var isTopFace = true;
        for (var i = 0; i < 2; ++i)
        {
            // Determine the orientation of the world-space plane on which the tile sprites are positioned.
            // This ensures that matching is done across the same surface (e.g. walls with walls, floors with
            // floors).
            var basisEast = GridPosition.OneX;
            var basisNorth = isTopFace ? GridPosition.OneY : GridPosition.OneZ;

            /* Identify neighbors. */
            var below = gridPosition - GridPosition.OneZ;
            var north = gridPosition + basisNorth;
            var south = gridPosition - basisNorth;
            var east = gridPosition + basisEast;
            var west = gridPosition - basisEast;
            var northEast = north + basisEast;
            var southEast = south + basisEast;
            var southWest = south - basisEast;
            var northWest = north - basisEast;

            // Update cache for all neighbors in the plane.
            UpdateCacheAtPosition(north, isTopFace);
            UpdateCacheAtPosition(south, isTopFace);
            UpdateCacheAtPosition(east, isTopFace);
            UpdateCacheAtPosition(west, isTopFace);
            UpdateCacheAtPosition(northEast, isTopFace);
            UpdateCacheAtPosition(southEast, isTopFace);
            UpdateCacheAtPosition(southWest, isTopFace);
            UpdateCacheAtPosition(northWest, isTopFace);
            if (isTopFace) UpdateCacheAtPosition(below, true);
            if (updateSelf) UpdateCacheAtPosition(gridPosition, isTopFace);

            isTopFace = !isTopFace;
        }
    }

    /// <summary>
    ///     Updates the cache entry for the block at the given position, if any.
    /// </summary>
    /// <param name="gridPosition">Position to update.</param>
    /// <param name="isTopFace">If true, update top face; otherwise update front face.</param>
    private void UpdateCacheAtPosition(GridPosition gridPosition, bool isTopFace)
    {
        /* Confirm that a block is present at the given position. */
        var blockIds = blockIndexer.GetEntitiesAtPosition(gridPosition);
        if (blockIds == null || blockIds.Count == 0) return;

        /* Resolve to the tile sprite level. */
        var blockId = blockIds.Keys.First();
        var centerId = GetTileSpriteIdForBlock(blockId, isTopFace);
        if (centerId < 0)
            // Block isn't ready yet, return at a later pass.
            return;

        // Determine the orientation of the world-space plane on which the tile sprites are positioned.
        // This ensures that matching is done across the same surface (e.g. walls with walls, floors with
        // floors).
        var basisEast = GridPosition.OneX;
        var basisNorth = isTopFace ? GridPosition.OneY : GridPosition.OneZ;

        /* Resolve neighbors to the tile sprite level. */
        var north = gridPosition + basisNorth;
        var south = gridPosition - basisNorth;
        var east = gridPosition + basisEast;
        var west = gridPosition - basisEast;
        var northEast = north + basisEast;
        var southEast = south + basisEast;
        var southWest = south - basisEast;
        var northWest = north - basisEast;

        var northId = GetTileSpriteIdForPosition(north, isTopFace);
        var southId = GetTileSpriteIdForPosition(south, isTopFace);
        var eastId = GetTileSpriteIdForPosition(east, isTopFace);
        var westId = GetTileSpriteIdForPosition(west, isTopFace);
        var northEastId = GetTileSpriteIdForPosition(northEast, isTopFace);
        var southEastId = GetTileSpriteIdForPosition(southEast, isTopFace);
        var southWestId = GetTileSpriteIdForPosition(southWest, isTopFace);
        var northWestId = GetTileSpriteIdForPosition(northWest, isTopFace);

        /* Resolve tile sprite to animated sprites. */
        var tileSprite = tileSpriteManager.TileSprites[centerId];
        var contextKey = new TileContextKey(northId, northEastId, eastId, southEastId, southId, southWestId, westId,
            northWestId);
        var resolvedSprites = tileSprite.GetMatchingAnimatedSpriteIds(contextKey);

        /* Retrieve and populate cache. */
        var dict = isTopFace ? topFaceCache : frontFaceCache;
        dict[blockId] = resolvedSprites;
    }

    /// <summary>
    ///     Gets the tile sprite ID for the block at the given position, if any.
    /// </summary>
    /// <param name="gridPosition">Grid position.</param>
    /// <param name="isTopFace">Whether to resolve the top face.</param>
    /// <returns>Resolved tile sprite ID, or TileSprite.Wildcard if no valid block.</returns>
    private int GetTileSpriteIdForPosition(GridPosition gridPosition, bool isTopFace)
    {
        var blockIds = blockIndexer.GetEntitiesAtPosition(gridPosition);
        if (blockIds == null || blockIds.Count == 0) return TileSprite.Empty;

        return GetTileSpriteIdForBlock(blockIds.Keys.First(), isTopFace);
    }

    /// <summary>
    ///     Gets the tile sprite ID for the given block.
    /// </summary>
    /// <param name="blockId">Block entity ID.</param>
    /// <param name="isTopFace">If true, resolve the top face; otherwise resolve the front face.</param>
    /// <returns>Resolved tile sprite ID.</returns>
    private int GetTileSpriteIdForBlock(ulong blockId, bool isTopFace)
    {
        if (!materials.HasComponentForEntity(blockId) || !materialModifiers.HasComponentForEntity(blockId))
            return TileSprite.Empty;

        /* Retrieve the tile sprite information. */
        try
        {
            var material = materialManager.Materials[materials[blockId]];
            var subtype = material.MaterialSubtypes[materialModifiers[blockId]];

            if (isTopFace)
            {
                var obscured = aboveBlocks.HasComponentForEntity(blockId);
                return obscured
                    ? subtype.ObscuredTopFaceTileSpriteId
                    : subtype.TopFaceTileSpriteId;
            }

            return subtype.SideFaceTileSpriteId;
        }
        catch
        {
            return TileSprite.Wildcard;
        }
    }

    /// <summary>
    ///     Updates the known position for the given block.
    /// </summary>
    /// <param name="blockId">Block to update.</param>
    private void UpdatePositionForBlock(ulong blockId)
    {
        if (!blockPositions.HasComponentForEntity(blockId))
        {
            Logger.ErrorFormat("Block {0} has no position.", blockId);
            return;
        }

        knownPositions[blockId] = blockPositions[blockId];
    }

    /// <summary>
    ///     Removes an entity from the cache entirely.
    /// </summary>
    /// <param name="blockId">Block entity ID to remove.</param>
    private void RemoveEntity(ulong blockId)
    {
        topFaceCache.TryRemove(blockId, out _);
        frontFaceCache.TryRemove(blockId, out _);
    }

    /// <summary>
    ///     Called when a materials update begins.
    /// </summary>
    private void OnStartUpdates()
    {
        if (updateCount == 0)
        {
            // First update pass for this tick, start new collections.
            changedBlocks = blockSetPool.TakeObject();
            removedBlocks = blockSetPool.TakeObject();
            changedBlocks.Clear();
            removedBlocks.Clear();

            changedBlocks.UnionWith(templateChanges.Keys.Where(entityId =>
                blockPositions.HasComponentForEntity(entityId) ||
                blockPositions.HasPendingComponentForEntity(entityId)));
            templateChanges.Clear();
        }

        updateCount++;
    }

    /// <summary>
    ///     Called when a materials update ends.
    /// </summary>
    private void OnEndUpdates()
    {
        /* Check whether both materials and modifiers have updated. */
        if (updateCount >= UpdatedCollectionCount)
        {
            /* Update the cache asynchronously. */
            if (refreshAll)
            {
                changedBlocks.UnionWith(frontFaceCache.Keys);
                refreshAll = false;
            }

            if (changedBlocks.Count > 0 || removedBlocks.Count > 0)
            {
                var changedSet = changedBlocks;
                var removedSet = removedBlocks;
                Task.Factory.StartNew(() => UpdateCache(changedSet, removedSet));
            }

            /* Reset state for the next tick. */
            updateCount = 0;
        }
    }

    /// <summary>
    ///     Called when a component is added.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="componentValue">Not used.</param>
    /// <param name="isLoad">Not used.</param>
    private void OnComponentAdded(ulong entityId, int componentValue, bool isLoad)
    {
        if (entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId)
            refreshAll = true;
        else
            changedBlocks.Add(entityId);
    }

    /// <summary>
    ///     Called when a component is modified.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="componentValue">Not used.</param>
    private void OnComponentModified(ulong entityId, int componentValue)
    {
        // Treat it as an add.
        OnComponentAdded(entityId, componentValue, false);
    }

    /// <summary>
    ///     Called when a component is removed.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="isUnload">Not used.</param>
    private void OnComponentRemoved(ulong entityId, bool isUnload)
    {
        removedBlocks.Add(entityId);
    }

    /// <summary>
    ///     Called when a block position is added or modified.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="componentValue">Not used.</param>
    /// <param name="isLoad">Not used.</param>
    private void OnPositionAdded(ulong entityId, GridPosition componentValue, bool isLoad)
    {
        OnComponentModified(entityId, 0);
    }

    /// <summary>
    ///     Called when a block position is modified.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="componentValue">Not used.</param>
    private void OnPositionModified(ulong entityId, GridPosition componentValue)
    {
        OnComponentModified(entityId, 0);
    }

    /// <summary>
    ///     Called when a block position is removed.
    /// </summary>
    /// <param name="entityId">Block entity ID.</param>
    /// <param name="isUnload">Not used.</param>
    private void OnPositionRemoved(ulong entityId, bool isUnload)
    {
        OnComponentRemoved(entityId, isUnload);
    }

    /// <summary>
    ///     Refreshes the entire cache in the background whenever resources are updated.
    /// </summary>
    /// <param name="id">Unused.</param>
    private void OnResourceChange(int id)
    {
        Task.Run(RefreshCache);
    }

    private void OnTemplateChange(ulong entityId, ulong templateEntityId)
    {
        templateChanges.TryAdd(entityId, 0);
    }

    /// <summary>
    ///     Registers the event handlers.
    /// </summary>
    private void RegisterEventHandlers()
    {
        materials.OnComponentAdded += OnComponentAdded;
        materials.OnComponentModified += OnComponentModified;
        materials.OnComponentRemoved += OnComponentRemoved;
        materialModifiers.OnComponentModified += OnComponentModified;
        blockPositions.OnComponentAdded += OnPositionAdded;
        blockPositions.OnComponentModified += OnPositionModified;
        blockPositions.OnComponentRemoved += OnPositionRemoved;
        entityManager.OnUpdatesStarted += OnStartUpdates;
        entityManager.OnUpdatesComplete += OnEndUpdates;
        tileSpriteManager.OnTileSpriteAdded += OnResourceChange;
        tileSpriteManager.OnTileSpriteUpdated += OnResourceChange;
        tileSpriteManager.OnTileSpriteRemoved += OnResourceChange;
        materialManager.OnMaterialAdded += OnResourceChange;
        materialManager.OnMaterialUpdated += OnResourceChange;
        materialManager.OnMaterialRemoved += OnResourceChange;
        entityTable.OnTemplateSet += OnTemplateChange;
    }

    /// <summary>
    ///     Deregisters the event handlers.
    /// </summary>
    private void DeregisterEventHandlers()
    {
        materials.OnComponentAdded -= OnComponentAdded;
        materials.OnComponentModified -= OnComponentModified;
        materials.OnComponentRemoved -= OnComponentRemoved;
        materialModifiers.OnComponentModified -= OnComponentModified;
        blockPositions.OnComponentAdded -= OnPositionAdded;
        blockPositions.OnComponentModified -= OnPositionModified;
        blockPositions.OnComponentRemoved -= OnPositionRemoved;
        entityManager.OnUpdatesComplete -= OnEndUpdates;
        tileSpriteManager.OnTileSpriteAdded -= OnResourceChange;
        tileSpriteManager.OnTileSpriteUpdated -= OnResourceChange;
        tileSpriteManager.OnTileSpriteRemoved -= OnResourceChange;
        materialManager.OnMaterialAdded -= OnResourceChange;
        materialManager.OnMaterialUpdated -= OnResourceChange;
        materialManager.OnMaterialRemoved -= OnResourceChange;
        entityTable.OnTemplateSet -= OnTemplateChange;
    }
}