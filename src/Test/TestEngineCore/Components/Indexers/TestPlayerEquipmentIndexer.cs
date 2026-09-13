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
///     Unit tests for PlayerEquipmentIndexer.
/// </summary>
public class TestPlayerEquipmentIndexer
{
    private const ulong PlayerId = 1;
    private const ulong EquipmentSlotId = 2;
    private const ulong OtherEquipmentSlotId = 3;
    private const ulong InventorySlotId = 4;

    private readonly EquipmentTypeComponentCollection equipmentTypes;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly ParentComponentCollection parents;
    private readonly EntityTable entityTable;
    private readonly PlayerEquipmentIndexer indexer;

    public TestPlayerEquipmentIndexer()
    {
        entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        entityTypes = new EntityTypeComponentCollection(entityTable, componentManager);
        equipmentTypes = new EquipmentTypeComponentCollection(entityTable, componentManager);
        parents = new ParentComponentCollection(entityTable, componentManager);
        indexer = new PlayerEquipmentIndexer(parents, entityTypes, equipmentTypes, entityTable,
            NullLogger<PlayerEquipmentIndexer>.Instance);
    }

    /// <summary>
    ///     Creates an entity of the given type with the given parent (if any).
    /// </summary>
    private void AddEntity(ulong entityId, EntityType entityType, ulong parentId, EquipmentType? equipmentType = null)
    {
        entityTable.Add(entityId, 0, false, false, false);
        entityTable.UpdateAllEntities();

        entityTypes.AddComponent(entityId, entityType);
        entityTypes.ApplyComponentUpdates();

        if (equipmentType.HasValue)
        {
            equipmentTypes.AddComponent(entityId, equipmentType.Value);
            equipmentTypes.ApplyComponentUpdates();
        }

        if (parentId != 0)
        {
            parents.AddComponent(entityId, parentId);
            parents.ApplyComponentUpdates();
        }
    }

    private void MoveEntity(ulong entityId, ulong newParentId)
    {
        parents.AddOrUpdateComponent(entityId, newParentId);
        parents.ApplyComponentUpdates();
    }

    private void RemoveEntity(ulong entityId)
    {
        entityTable.Remove(entityId, false);
        entityTable.UpdateAllEntities();
    }

    [Fact]
    public void EquippingItem_SetsArrayEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Weapon);

        const ulong itemId = 10;
        AddEntity(itemId, EntityType.Item, 0, EquipmentType.Weapon);
        MoveEntity(itemId, EquipmentSlotId);

        Assert.True(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Weapon, out var equipped));
        Assert.Equal(itemId, equipped);
    }

    [Fact]
    public void MovingItemToInventorySlot_ClearsEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Weapon);
        AddEntity(InventorySlotId, EntityType.Slot, PlayerId);

        const ulong itemId = 10;
        AddEntity(itemId, EntityType.Item, 0, EquipmentType.Weapon);
        MoveEntity(itemId, EquipmentSlotId);
        MoveEntity(itemId, InventorySlotId);

        Assert.True(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Weapon, out var equipped));
        Assert.Equal(0UL, equipped);
    }

    [Fact]
    public void RemovingParentComponent_ClearsEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Offhand);

        const ulong itemId = 10;
        AddEntity(itemId, EntityType.Item, 0, EquipmentType.Offhand);
        MoveEntity(itemId, EquipmentSlotId);
        Assert.True(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Offhand, out var equipped));
        Assert.Equal(itemId, equipped);

        parents.RemoveComponent(itemId);
        parents.ApplyComponentUpdates();

        Assert.True(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Offhand, out var cleared));
        Assert.Equal(0UL, cleared);
    }

    [Fact]
    public void NonItemChildOfEquipmentSlot_IsIgnored()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Helmet);

        // An NPC child of an equipment slot is not tracked.
        AddEntity(10, EntityType.Npc, EquipmentSlotId);
        Assert.False(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Helmet, out _));

        // An item without an EquipmentType component is not tracked either.
        AddEntity(11, EntityType.Item, EquipmentSlotId);
        Assert.False(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Helmet, out _));
    }

    [Fact]
    public void OwnerEntityRemoval_DropsEntry()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Chest);

        const ulong itemId = 10;
        AddEntity(itemId, EntityType.Item, 0, EquipmentType.Chest);
        MoveEntity(itemId, EquipmentSlotId);
        Assert.True(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Chest, out _));

        RemoveEntity(PlayerId);
        Assert.False(indexer.TryGetEquippedItem(PlayerId, EquipmentType.Chest, out _));
    }

    [Fact]
    public void DirectArrayAccess_ReflectsEquippedItems()
    {
        AddEntity(PlayerId, EntityType.Player, 0);
        AddEntity(EquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.Weapon);
        AddEntity(OtherEquipmentSlotId, EntityType.EquipmentSlot, PlayerId, EquipmentType.RightRing);

        const ulong weaponId = 10;
        AddEntity(weaponId, EntityType.Item, 0, EquipmentType.Weapon);
        MoveEntity(weaponId, EquipmentSlotId);

        const ulong ringId = 11;
        AddEntity(ringId, EntityType.Item, 0, EquipmentType.RightRing);
        MoveEntity(ringId, OtherEquipmentSlotId);

        Assert.True(indexer.TryGetEquipmentForPlayer(PlayerId, out var equipment));
        Assert.Equal(weaponId, equipment[(int)EquipmentType.Weapon]);
        Assert.Equal(ringId, equipment[(int)EquipmentType.RightRing]);
        Assert.Equal(0UL, equipment[(int)EquipmentType.Helmet]);
    }
}
