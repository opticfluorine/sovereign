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

using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Xunit;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Unit tests for EquipmentSlotIndexer.
/// </summary>
public class TestEquipmentSlotIndexer
{
    private const ulong PlayerId = 1;
    private const ulong EquipmentSlotId = 2;
    private const ulong InventorySlotId = 3;

    private readonly EquipmentTypeComponentCollection equipmentTypes;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly ParentComponentCollection parents;
    private readonly EntityTable entityTable;
    private readonly EquipmentSlotIndexer indexer;

    public TestEquipmentSlotIndexer()
    {
        entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        entityTypes = new EntityTypeComponentCollection(entityTable, componentManager);
        parents = new ParentComponentCollection(entityTable, componentManager);
        equipmentTypes = new EquipmentTypeComponentCollection(entityTable, componentManager);
        indexer = new EquipmentSlotIndexer(equipmentTypes, entityTypes, parents, entityTable,
            NullLogger<EquipmentSlotIndexer>.Instance);
    }

    /// <summary>
    ///     Creates an entity of the given type with the given parent (if any).
    ///     Components are applied in canonical order so that the EquipmentType event,
    ///     on which the indexer keys, fires last.
    /// </summary>
    private void AddEntity(ulong entityId, EntityType entityType, ulong parentId, EquipmentType? equipmentType = null)
    {
        entityTable.Add(entityId, 0, false, false, false);
        entityTable.UpdateAllEntities();

        entityTypes.AddComponent(entityId, entityType);
        entityTypes.ApplyComponentUpdates();

        if (parentId != 0)
        {
            parents.AddComponent(entityId, parentId);
            parents.ApplyComponentUpdates();
        }

        if (equipmentType.HasValue)
        {
            equipmentTypes.AddComponent(entityId, equipmentType.Value);
            equipmentTypes.ApplyComponentUpdates();
        }
    }

    private void RemoveEquipmentType(ulong entityId)
    {
        equipmentTypes.RemoveComponent(entityId);
        equipmentTypes.ApplyComponentUpdates();
    }

    private void ChangeEquipmentType(ulong entityId, EquipmentType newEquipmentType)
    {
        equipmentTypes.AddOrUpdateComponent(entityId, newEquipmentType);
        equipmentTypes.ApplyComponentUpdates();
    }

    private void RemoveEntity(ulong entityId)
    {
        entityTable.Remove(entityId, false);
        entityTable.UpdateAllEntities();
    }

    [Fact]
    public void AddingEquipmentSlot_TiesSlotToOwner()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Weapon);

        Assert.True(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Weapon, out var slotId));
        Assert.Equal(EquipmentSlotId, slotId);
        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Helmet, out _));
    }

    [Fact]
    public void SlotWithoutEquipmentType_IsIgnored()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId);

        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Weapon, out _));
    }

    [Fact]
    public void NonSlotEntity_IsIgnored()
    {
        AddEntity(PlayerId, EntityType.Player, 0);

        // An item with an EquipmentType component is not an equipment slot.
        AddEntity(10, EntityType.Item, PlayerId, EquipmentType.Weapon);
        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Weapon, out _));
    }

    [Fact]
    public void SlotWithoutParent_IsIgnored()
    {
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, 0, EquipmentType.Weapon);

        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Weapon, out _));
    }

    [Fact]
    public void RemovingEquipmentType_ClearsEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Offhand);
        Assert.True(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Offhand, out _));

        RemoveEquipmentType(EquipmentSlotId);

        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Offhand, out _));
    }

    [Fact]
    public void ChangingEquipmentType_MovesEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Weapon);

        ChangeEquipmentType(EquipmentSlotId, EquipmentType.Helmet);

        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Weapon, out _));
        Assert.True(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Helmet, out var slotId));
        Assert.Equal(EquipmentSlotId, slotId);
    }

    [Fact]
    public void OwnerEntityRemoval_DropsEntries()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Chest);
        AddEntity(InventorySlotId, EntityType.Slot, PlayerId);
        Assert.True(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Chest, out _));

        RemoveEntity(PlayerId);

        Assert.False(indexer.TryGetEquipmentSlot(PlayerId, EquipmentType.Chest, out _));
    }
}
