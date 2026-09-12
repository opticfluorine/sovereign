// Sovereign Engine
// Copyright (c) 2026 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

// ReSharper disable InconsistentlySynchronizedField
// -- Disabled as ReSharper doesn't understand our pattern of taking/releasing the lock when
// -- component updates are in progress to avoid many redundant lock/unlock cycles

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Tracks the items equipped by each player, indexed by player entity ID and equipment type.
/// </summary>
/// <remarks>
///     This class is thread-safe.
/// </remarks>
public sealed class PlayerEquipmentIndexer : BaseComponentIndexer<ulong>
{
    private readonly Lock accessLock = new();
    private readonly EquipmentTypeComponentCollection equipmentTypes;
    private readonly ILogger<PlayerEquipmentIndexer> logger;

    /// <summary>
    ///     Equipped item IDs by player entity ID and equipment type index.
    ///     A value of 0 indicates an empty slot.
    /// </summary>
    private readonly Dictionary<ulong, ulong[]> equippedByPlayer = new();

    /// <summary>
    ///     Maps each tracked item to the (owner, equipment type) pair of the equipment slot
    ///     it is currently parented to, if any.
    /// </summary>
    private readonly Dictionary<ulong, (ulong OwnerId, int SlotIndex)> equippedSlotByItem = new();

    private readonly EntityTypeComponentCollection entityTypes;
    private readonly ParentComponentCollection parents;

    public PlayerEquipmentIndexer(ParentComponentCollection parents,
        EntityTypeComponentCollection entityTypes, EquipmentTypeComponentCollection equipmentTypes,
        EntityTable entityTable, ILogger<PlayerEquipmentIndexer> logger)
        : base(parents, parents)
    {
        this.parents = parents;
        this.entityTypes = entityTypes;
        this.equipmentTypes = equipmentTypes;
        this.logger = logger;

        entityTable.OnEntityRemoved += OnEntityRemoved;
    }

    /// <summary>
    ///     Gets the item equipped by a player in the given equipment type slot.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="type">Equipment type.</param>
    /// <param name="itemId">Equipped item entity ID, or 0 if nothing is equipped in that slot.</param>
    /// <returns>true if the player has equipment entries, false if the player is unknown to the indexer.</returns>
    public bool TryGetEquippedItem(ulong playerId, EquipmentType type, out ulong itemId)
    {
        itemId = 0;
        lock (accessLock)
        {
            if (!equippedByPlayer.TryGetValue(playerId, out var equipment)) return false;
            itemId = equipment[(int)type];
            return true;
        }
    }

    /// <summary>
    ///     Gets the equipped items for a player as an array indexed by equipment type.
    ///     The returned array is live shared state and must be treated as read-only by callers.
    ///     A value of 0 indicates an empty slot.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="equipment">Equipped item array indexed by equipment type.</param>
    /// <returns>true if the player has equipment entries, false otherwise.</returns>
    public bool TryGetEquipmentForPlayer(ulong playerId, out ulong[] equipment)
    {
        lock (accessLock)
        {
            return equippedByPlayer.TryGetValue(playerId, out equipment!);
        }
    }

    protected override void StartUpdatesCallback()
    {
        accessLock.Enter();
    }

    protected override void EndUpdatesCallback()
    {
        accessLock.Exit();
    }

    protected override void ComponentAddedCallback(ulong entityId, ulong parentId, bool isLoad)
    {
        UpdateEquippedItem(entityId, parentId);
    }

    protected override void ComponentModifiedCallback(ulong entityId, ulong parentId)
    {
        UpdateEquippedItem(entityId, parentId);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        // The item's parent component is gone (e.g. the item was dropped into the world),
        // so it is no longer equipped regardless of its previous parent.
        ClearTrackedEntry(entityId);
    }

    /// <summary>
    ///     Updates the equipped-item tracking for an item whose parent has changed.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="itemId">Item entity ID.</param>
    /// <param name="newParentId">New parent entity ID.</param>
    private void UpdateEquippedItem(ulong itemId, ulong newParentId)
    {
        if (!IsTrackedItem(itemId))
        {
            ClearTrackedEntry(itemId);
            return;
        }

        if (!IsEquipmentSlotWithOwner(newParentId, out var ownerId, out var slotIndex))
        {
            // The item is not parented to an equipment slot, so it is not equipped.
            ClearTrackedEntry(itemId);
            return;
        }

        GetOrAddEquipmentForPlayer(ownerId)[slotIndex] = itemId;
        equippedSlotByItem[itemId] = (ownerId, slotIndex);
    }

    /// <summary>
    ///     Clears the equipped-item entry for an item that is no longer parented to an
    ///     equipment slot, zeroing the array element only if it still holds the item.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="itemId">Item entity ID.</param>
    private void ClearTrackedEntry(ulong itemId)
    {
        if (!equippedSlotByItem.Remove(itemId, out var slot)) return;
        if (!equippedByPlayer.TryGetValue(slot.OwnerId, out var equipment)) return;
        if (equipment[slot.SlotIndex] == itemId) equipment[slot.SlotIndex] = 0;
    }

    /// <summary>
    ///     Determines whether the parent entity is an equipment slot with a resolvable owner.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="slotId">Candidate equipment slot entity ID.</param>
    /// <param name="ownerId">Owner entity ID of the slot.</param>
    /// <param name="slotIndex">Equipment type index of the slot.</param>
    /// <returns>true if the entity is an equipment slot with an owner, false otherwise.</returns>
    private bool IsEquipmentSlotWithOwner(ulong slotId, out ulong ownerId, out int slotIndex)
    {
        ownerId = 0;
        slotIndex = 0;

        if (!entityTypes.TryGetValue(slotId, out var slotType) ||
            slotType != EntityType.EquipmentSlot ||
            !parents.TryGetValue(slotId, out ownerId)) return false;

        if (!equipmentTypes.TryGetValue(slotId, out var slotEquipmentType))
        {
            logger.LogWarning("Equipment slot {Slot} has no EquipmentType; item cannot be tracked as equipped.",
                slotId);
            return false;
        }

        slotIndex = (int)slotEquipmentType;
        return true;
    }

    /// <summary>
    ///     Determines whether an entity is an item with an EquipmentType component.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if the entity is an equippable item, false otherwise.</returns>
    private bool IsTrackedItem(ulong entityId)
    {
        return entityTypes.TryGetValue(entityId, out var entityType) &&
               entityType == EntityType.Item &&
               equipmentTypes.HasComponentForEntity(entityId);
    }

    /// <summary>
    ///     Gets the equipment array for a player, creating it if needed.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <returns>Equipment array.</returns>
    private ulong[] GetOrAddEquipmentForPlayer(ulong playerId)
    {
        if (equippedByPlayer.TryGetValue(playerId, out var equipment)) return equipment;

        equipment = new ulong[EquipmentConstants.EquipmentSlotCount];
        equippedByPlayer[playerId] = equipment;
        return equipment;
    }

    /// <summary>
    ///     Called when an entity is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="unused">Unused.</param>
    private void OnEntityRemoved(ulong entityId, bool unused)
    {
        lock (accessLock)
        {
            equippedByPlayer.Remove(entityId);
            equippedSlotByItem.Remove(entityId);
        }
    }
}
