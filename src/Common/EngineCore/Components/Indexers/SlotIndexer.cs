// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexes slot entities by their parent entity to which they belong.
/// </summary>
/// <remarks>
///     This class is thread-safe.
/// </remarks>
public sealed class SlotIndexer : BaseComponentIndexer<EntityType>
{
    private const int DefaultSize = 8192;
    private readonly Lock accessLock = new();
    private readonly HashSet<ulong> entitiesWithPendingSlot = new();
    private readonly EntityHierarchyIndexer hierarchyIndexer;
    private readonly ILogger<SlotIndexer> logger;

    private readonly ParentComponentCollection parents;

    private readonly Dictionary<ulong, List<ulong>> slotsByParent = new(DefaultSize);

    public SlotIndexer(EntityTypeComponentCollection entityTypes, SlotComponentEventFilter filter,
        ParentComponentCollection parents, ILogger<SlotIndexer> logger, EntityTable entityTable,
        EntityHierarchyIndexer hierarchyIndexer)
        : base(entityTypes, filter)
    {
        this.parents = parents;
        this.logger = logger;
        this.hierarchyIndexer = hierarchyIndexer;

        entityTable.OnEntityRemoved += OnEntityUnloaded;
    }

    /// <summary>
    ///     Gets the IDs of any slots attached directly to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slots">List to which slot IDs will be added in ascending order.</param>
    public void GetSlotsForEntity(ulong entityId, List<ulong> slots)
    {
        lock (accessLock)
        {
            if (!slotsByParent.TryGetValue(entityId, out var knownSlots)) return;
            slots.AddRange(knownSlots);
        }
    }

    /// <summary>
    ///     Gets a specific slot attached to an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="slotId">Slot entity ID. Only meaningful if method returns true.</param>
    /// <returns>true if the slot was found, false otherwise.</returns>
    public bool TryGetSlotForEntity(ulong entityId, int slotIndex, out ulong slotId)
    {
        slotId = 0;
        if (slotIndex < 0) return false;

        lock (accessLock)
        {
            if (!slotsByParent.TryGetValue(entityId, out var knownSlots)) return false;
            if (slotIndex >= knownSlots.Count) return false;
            slotId = knownSlots[slotIndex];
        }

        return true;
    }

    /// <summary>
    ///     Finds the first empty slot on an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slotId">Slot ID. Only meaningful if method returns true.</param>
    /// <returns>Whether an empty slot was found on the entity.</returns>
    /// <remarks>
    ///     This method will only return a slot for a given entity once per tick. This is because if a free slot
    ///     is filled with an item during a tick, the parent-child relationship will not be committed until the
    ///     end of a tick. In this case, the method will return false even if another free slot exists.
    /// </remarks>
    public bool TryFindEmptySlot(ulong entityId, out ulong slotId)
    {
        slotId = 0;

        lock (accessLock)
        {
            if (!entitiesWithPendingSlot.Add(entityId)) return false;
            if (!slotsByParent.TryGetValue(entityId, out var slotList)) return false;
            foreach (var checkId in slotList)
                if (!hierarchyIndexer.TryGetFirstDirectChild(checkId, out _))
                {
                    slotId = checkId;
                    return true;
                }
        }

        return false;
    }

    protected override void StartUpdatesCallback()
    {
        accessLock.Enter();
    }

    protected override void EndUpdatesCallback()
    {
        entitiesWithPendingSlot.Clear();
        accessLock.Exit();
    }

    protected override void ComponentAddedCallback(ulong entityId, EntityType componentValue, bool isLoad)
    {
        if (!parents.TryGetValue(entityId, out var parentId))
        {
            logger.LogWarning("Slot {Id:X} has no parent.", entityId);
            return;
        }

        // Lock is already held - do not relock. Warnings here about synchronization are false alarms.

        if (!slotsByParent.TryGetValue(parentId, out var slotList))
        {
            slotList = new List<ulong>();
            slotsByParent[parentId] = slotList;
        }

        var where = ~slotList.BinarySearch(entityId);
        if (where < 0)
        {
            logger.LogWarning("Duplicate slot ID {Id:X} for entity {PId:X}.", entityId, parentId);
            return;
        }

        slotList.Insert(where, entityId);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        if (!parents.HasComponentForEntity(entityId, true)) return;
        var parentId = parents.GetComponentWithLookback(entityId);

        // Lock is already held - do not relock. Warnings here about synchronization are false alarms.

        if (!slotsByParent.TryGetValue(parentId, out var slotList)) return;
        slotList.Remove(entityId);
    }

    /// <summary>
    ///     Called when an entity is unloaded or removed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="unused">Unused.</param>
    private void OnEntityUnloaded(ulong entityId, bool unused)
    {
        lock (accessLock)
        {
            slotsByParent.Remove(entityId);
        }
    }
}