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

using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

[MessagePackObject]
public class InventorySwapEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID that performs the swap.
    /// </summary>
    [IgnoreMember]
    public ulong ActorId { get; set; }

    /// <summary>
    ///     Entity ID that owns the inventory containing the source slot.
    /// </summary>
    [Key(0)]
    public ulong FirstInventoryEntityId { get; set; }

    /// <summary>
    ///     Slot index to swap items from.
    /// </summary>
    [Key(1)]
    public int FirstSlotIndex { get; set; }

    /// <summary>
    ///     Entity ID that owns the inventory containing the destination slot.
    /// </summary>
    [Key(2)]
    public ulong SecondInventoryEntityId { get; set; }

    /// <summary>
    ///     Slot index to swap items to.
    /// </summary>
    [Key(3)]
    public int SecondSlotIndex { get; set; }

    /// <summary>
    ///     Quantity of items to move from the source slot to the destination slot.
    /// </summary>
    [Key(4)]
    public uint Quantity { get; set; }
}