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
    /// <param name="itemId">Item ID.</param>
    void PickUp(IEventSender eventSender, ulong itemId);

    /// <summary>
    ///     Drops the given item.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="itemId">Item ID.</param>
    void Drop(IEventSender eventSender, ulong itemId);

    /// <summary>
    ///     Swaps the contents of two inventory slots.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="firstSlotIdx">First slot index.</param>
    /// <param name="secondSlotIdx">Second slot index.</param>
    void Swap(IEventSender eventSender, int firstSlotIdx, int secondSlotIdx);
}

/// <summary>
///     Implementation of IInventoryController.
/// </summary>
internal class InventoryController : IInventoryController
{
    public void PickUp(IEventSender eventSender, ulong itemId)
    {
        var details = new EntityEventDetails { EntityId = itemId };
        var ev = new Event(EventId.Core_Inventory_PickUp, details);
        eventSender.SendEvent(ev);
    }

    public void Drop(IEventSender eventSender, ulong itemId)
    {
        var details = new EntityEventDetails { EntityId = itemId };
        var ev = new Event(EventId.Core_Inventory_Drop, details);
        eventSender.SendEvent(ev);
    }

    public void Swap(IEventSender eventSender, int firstSlotIdx, int secondSlotIdx)
    {
        var details = new IntPairEventDetails { First = firstSlotIdx, Second = secondSlotIdx };
        var ev = new Event(EventId.Core_Inventory_Swap, details);
        eventSender.SendEvent(ev);
    }
}