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

using System;
using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineUtil.Attributes;
using Sovereign.Scripting.Lua;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Lua scripting library for inventory.
/// </summary>
[ScriptableLibrary("Inventory")]
public sealed class InventoryScripting(
    IInventoryController controller,
    IEventSender eventSender,
    IInventoryServices services,
    SlotIndexer slotIndexer,
    ParentComponentCollection parents,
    EntityTypeComponentCollection entityTypes)
{
    [ScriptableFunction("PickUp")]
    public void PickUp(ulong entityId, ulong itemEntityId)
    {
        if (!EntityUtil.IsRegularEntity(itemEntityId))
            throw new LuaException("itemEntityId must be a regular entity");
        if (!entityTypes.TryGetValue(itemEntityId, out var entityType) || entityType != EntityType.Item)
            throw new LuaException("itemEntityId must be an item entity");

        controller.PickUp(eventSender, entityId, itemEntityId);
    }

    [ScriptableFunction("Drop")]
    public void Drop(ulong entityId, int slotIndex)
    {
        controller.Drop(eventSender, entityId, slotIndex - 1);
    }

    [ScriptableFunction("DropAt")]
    public void DropAt(ulong entityId, int slotIndex, Vector3 dropPosition)
    {
        controller.Drop(eventSender, entityId, slotIndex - 1, dropPosition);
    }

    [ScriptableFunction("Swap")]
    public void Swap(ulong entityId, int slotIndex0, int slotIndex1)
    {
        controller.Swap(eventSender, entityId, slotIndex0 - 1, slotIndex1 - 1);
    }

    [ScriptableFunction("RemoveItem")]
    public void RemoveItem(ulong entityId, int slotIndex)
    {
        controller.RemoveItem(eventSender, entityId, slotIndex - 1);
    }

    [ScriptableFunction("CanPickUp")]
    public bool CanPickUp(ulong entityId, ulong itemId)
    {
        return services.CanPickUp(entityId, itemId);
    }

    [ScriptableFunction("GetSlotCount")]
    public int GetSlotCount(ulong entityId)
    {
        return services.GetSlotCount(entityId);
    }

    [ScriptableFunction("GetInventory")]
    public void GetInventory(ulong entityId, [ScriptableOutputBuffer("GetSlotCount(entityId)")] Span<ulong> inventory)
    {
        services.GetInventory(entityId, inventory);
    }

    [ScriptableFunction("GetItem")]
    public ulong GetItem(ulong entityId, int slotIndex)
    {
        return services.GetItem(entityId, slotIndex - 1);
    }

    [ScriptableFunction("GetSlotIndexForItem")]
    public int GetSlotIndexForItem(ulong entityId, ulong itemId)
    {
        return services.GetSlotIndexForItem(entityId, itemId) + 1;
    }

    [ScriptableFunction("FindFirstMatchingItem")]
    public ulong FindFirstMatchingItem(ulong entityId, ulong templateId)
    {
        return services.FindFirstMatchingItem(entityId, templateId);
    }

    [ScriptableFunction("AddSlots")]
    public void AddSlots(ulong entityId, int slotCount)
    {
        controller.AddSlots(eventSender, entityId, slotCount);
    }

    [ScriptableFunction("GetEmptySlot")]
    public int GetEmptySlot(ulong entityId)
    {
        return slotIndexer.TryFindEmptySlot(entityId, out _, out var slotIndex) ? slotIndex + 1 : 0;
    }

    [ScriptableFunction("HasEmptySlot")]
    public bool HasEmptySlot(ulong entityId)
    {
        return slotIndexer.HasEmptySlot(entityId);
    }

    [ScriptableFunction("AddItem")]
    public bool AddItem(ulong entityId, ulong itemId)
    {
        if (!EntityUtil.IsRegularEntity(itemId)) throw new LuaException("itemId must be a regular entity.");
        if (!entityTypes.TryGetValue(itemId, out var entityType) || entityType != EntityType.Item)
            throw new LuaException("itemId must be an item entity.");

        if (!slotIndexer.TryFindEmptySlot(entityId, out var slotId, out _))
            return false;

        parents.AddOrUpdateComponent(itemId, slotId);
        return true;
    }
}