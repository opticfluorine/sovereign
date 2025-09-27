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
    private readonly ILogger<SlotIndexer> logger;

    private readonly ParentComponentCollection parents;

    private readonly Dictionary<ulong, List<ulong>> slotsByParent = new(DefaultSize);

    public SlotIndexer(EntityTypeComponentCollection entityTypes, SlotComponentEventFilter filter,
        ParentComponentCollection parents, ILogger<SlotIndexer> logger, EntityTable entityTable)
        : base(entityTypes, filter)
    {
        this.parents = parents;
        this.logger = logger;

        entityTable.OnEntityRemoved += OnEntityUnloaded;
    }

    /// <summary>
    ///     Gets the IDs of any slots attached directly to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="slots">List to which slot IDs will be added.</param>
    public void GetSlotsForEntity(ulong entityId, List<ulong> slots)
    {
        lock (accessLock)
        {
            if (!slotsByParent.TryGetValue(entityId, out var knownSlots)) return;
            slots.AddRange(knownSlots);
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

        slotList.Add(entityId);
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