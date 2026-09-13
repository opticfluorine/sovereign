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
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Tracks the equipment slots attached to each player, indexed by owner entity ID and equipment type.
/// </summary>
/// <remarks>
///     This class is thread-safe.
/// </remarks>
public sealed class EquipmentSlotIndexer : BaseComponentIndexer<EquipmentType>
{
    private readonly Lock accessLock = new();
    private readonly EquipmentTypeComponentCollection equipmentTypes;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly ILogger<EquipmentSlotIndexer> logger;

    /// <summary>
    ///     Maps each tracked equipment slot to the (owner, equipment type) pair of the player
    ///     it is currently parented to, if any.
    /// </summary>
    private readonly Dictionary<ulong, (ulong OwnerId, int SlotIndex)> ownerBySlot = new();

    private readonly ParentComponentCollection parents;

    /// <summary>
    ///     Equipment slot entity IDs by owner entity ID and equipment type index.
    ///     A value of 0 indicates no slot.
    /// </summary>
    private readonly Dictionary<ulong, ulong[]> slotsByOwner = new();

    public EquipmentSlotIndexer(EquipmentTypeComponentCollection equipmentTypes,
        EntityTypeComponentCollection entityTypes, ParentComponentCollection parents,
        EntityTable entityTable, ILogger<EquipmentSlotIndexer> logger)
        : base(equipmentTypes, equipmentTypes)
    {
        this.equipmentTypes = equipmentTypes;
        this.entityTypes = entityTypes;
        this.parents = parents;
        this.logger = logger;

        entityTable.OnEntityRemoved += OnEntityRemoved;
    }

    /// <summary>
    ///     Gets the equipment slot of the given owner for the given equipment type.
    /// </summary>
    /// <param name="ownerId">Owner entity ID.</param>
    /// <param name="equipmentType">Equipment type.</param>
    /// <param name="slotId">Equipment slot entity ID. Only meaningful if this method returns true.</param>
    /// <returns>true if the equipment slot was found, false otherwise.</returns>
    public bool TryGetEquipmentSlot(ulong ownerId, EquipmentType equipmentType, out ulong slotId)
    {
        slotId = 0;
        lock (accessLock)
        {
            if (!slotsByOwner.TryGetValue(ownerId, out var slots)) return false;
            slotId = slots[(int)equipmentType];
            return slotId != 0;
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

    protected override void ComponentAddedCallback(ulong entityId, EquipmentType componentValue, bool isLoad)
    {
        UpdateTrackedSlot(entityId);
    }

    protected override void ComponentModifiedCallback(ulong entityId, EquipmentType componentValue)
    {
        UpdateTrackedSlot(entityId);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        // The entity no longer has an equipment type, so it is no longer tracked.
        ClearTrackedEntry(entityId);
    }

    /// <summary>
    ///     Updates the equipment slot tracking for an entity whose EquipmentType component changed.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="slotId">Candidate equipment slot entity ID.</param>
    private void UpdateTrackedSlot(ulong slotId)
    {
        if (!IsTrackedEquipmentSlot(slotId, out var slotIndex))
        {
            ClearTrackedEntry(slotId);
            return;
        }

        if (!parents.TryGetValue(slotId, out var ownerId))
        {
            logger.LogWarning("Equipment slot {Slot} has no parent; it cannot be indexed.", slotId);
            ClearTrackedEntry(slotId);
            return;
        }

        // Release the previous tracking entry, if any, before overwriting it.
        if (ownerBySlot.Remove(slotId, out var previous) &&
            slotsByOwner.TryGetValue(previous.OwnerId, out var previousSlots) &&
            previousSlots[previous.SlotIndex] == slotId)
        {
            previousSlots[previous.SlotIndex] = 0;
        }

        GetOrAddSlotsForOwner(ownerId)[slotIndex] = slotId;
        ownerBySlot[slotId] = (ownerId, slotIndex);
    }

    /// <summary>
    ///     Clears the tracking entry for an equipment slot that is no longer an equipment slot
    ///     with a parent, zeroing the owner's array element only if it still holds the slot.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="slotId">Equipment slot entity ID.</param>
    private void ClearTrackedEntry(ulong slotId)
    {
        if (!ownerBySlot.Remove(slotId, out var slot)) return;
        if (!slotsByOwner.TryGetValue(slot.OwnerId, out var slots)) return;
        if (slots[slot.SlotIndex] == slotId) slots[slot.SlotIndex] = 0;
    }

    /// <summary>
    ///     Determines whether an entity is an equipment slot.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slotIndex">Equipment type index of the slot.</param>
    /// <returns>true if the entity is an equipment slot, false otherwise.</returns>
    private bool IsTrackedEquipmentSlot(ulong entityId, out int slotIndex)
    {
        slotIndex = 0;

        if (!entityTypes.TryGetValue(entityId, out var entityType) ||
            entityType != EntityType.EquipmentSlot ||
            !equipmentTypes.TryGetValue(entityId, out var equipmentType))
            return false;

        slotIndex = (int)equipmentType;
        return true;
    }

    /// <summary>
    ///     Gets the equipment slot array for an owner, creating it if needed.
    ///     Lock is already held - do not relock.
    /// </summary>
    /// <param name="ownerId">Owner entity ID.</param>
    /// <returns>Equipment slot array.</returns>
    private ulong[] GetOrAddSlotsForOwner(ulong ownerId)
    {
        if (slotsByOwner.TryGetValue(ownerId, out var slots)) return slots;

        slots = new ulong[InventoryConstants.EquipmentSlotCount];
        slotsByOwner[ownerId] = slots;
        return slots;
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
            slotsByOwner.Remove(entityId);
            ClearTrackedEntry(entityId);
        }
    }
}
