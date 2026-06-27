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

using System.Numerics;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Interface for interacting with the Inventory system.
/// </summary>
public interface IInventoryController
{
    /// <summary>
    ///     Picks up the given item.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that will pick up the item..</param>
    /// <param name="itemId">Item ID.</param>
    void PickUp(IEventSender eventSender, ulong entityId, ulong itemId);

    /// <summary>
    ///     Drops the given item.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that will drop the item.</param>
    /// <param name="slotIndex">Inventory slot index.</param>
    void Drop(IEventSender eventSender, ulong entityId, int slotIndex);

    /// <summary>
    ///     Drops the given item at the specified position.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that will drop the item.</param>
    /// <param name="slotIndex">Inventory slot index.</param>
    /// <param name="position">Requested drop position.</param>
    void Drop(IEventSender eventSender, ulong entityId, int slotIndex, Vector3 position);

    /// <summary>
    ///     Swaps the contents of two inventory slots.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that will swap the items.</param>
    /// <param name="firstSlotIdx">First slot index.</param>
    /// <param name="secondSlotIdx">Second slot index.</param>
    void Swap(IEventSender eventSender, ulong entityId, int firstSlotIdx, int secondSlotIdx);

    /// <summary>
    ///     Removes the item in an inventory slot.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that owns the inventory.</param>
    /// <param name="slotIndex">Slot index to remove entity from.</param>
    void RemoveItem(IEventSender eventSender, ulong entityId, int slotIndex);

    /// <summary>
    ///     Adds slots to an entity's inventory.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID that will own the new slots.</param>
    /// <param name="slotCount">Number of slots to add. Must be greater than zero.</param>
    void AddSlots(IEventSender eventSender, ulong entityId, int slotCount);
}

/// <summary>
///     Implementation of IInventoryController.
/// </summary>
internal class InventoryController : IInventoryController
{
    public void PickUp(IEventSender eventSender, ulong entityId, ulong itemId)
    {
        var details = new EntityEventDetails { EntityId = itemId };
        var ev = new Event(EventId.Core_Inventory_PickUp, details)
        {
            FromPlayerId = entityId
        };
        eventSender.SendEvent(ev);
    }

    public void Drop(IEventSender eventSender, ulong entityId, int slotIndex)
    {
        var details = new IntEventDetails { Value = (uint)slotIndex };
        var ev = new Event(EventId.Core_Inventory_Drop, details)
        {
            FromPlayerId = entityId
        };
        eventSender.SendEvent(ev);
    }

    public void Drop(IEventSender eventSender, ulong entityId, int slotIndex, Vector3 position)
    {
        var details = new IntVectorEventDetails
        {
            IntValue = slotIndex,
            VectorValue = position
        };
        var ev = new Event(EventId.Core_Inventory_DropAtPosition, details)
        {
            FromPlayerId = entityId
        };
        eventSender.SendEvent(ev);
    }

    public void Swap(IEventSender eventSender, ulong entityId, int firstSlotIdx, int secondSlotIdx)
    {
        var details = new IntPairEventDetails { First = firstSlotIdx, Second = secondSlotIdx };
        var ev = new Event(EventId.Core_Inventory_Swap, details)
        {
            FromPlayerId = entityId
        };
        eventSender.SendEvent(ev);
    }

    public void RemoveItem(IEventSender eventSender, ulong entityId, int slotIndex)
    {
        var details = new EntityIntEventDetails { EntityId = entityId, Value = slotIndex };
        var ev = new Event(EventId.Core_Inventory_RemoveItem, details);
        eventSender.SendEvent(ev);
    }

    public void AddSlots(IEventSender eventSender, ulong entityId, int slotCount)
    {
        var details = new EntityIntEventDetails { EntityId = entityId, Value = slotCount };
        var ev = new Event(EventId.Core_Inventory_AddSlots, details);
        eventSender.SendEvent(ev);
    }
}