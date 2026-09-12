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

/// <summary>
///     Details of an event requesting that a player equip the item in one of their inventory slots.
/// </summary>
[MessagePackObject]
public class EquipEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID of the player performing the equip. Overwritten by the server with the
    ///     authenticated player's entity ID when received over the network.
    /// </summary>
    [Key(0)]
    public ulong PlayerEntityId { get; set; }

    /// <summary>
    ///     Index of the inventory slot holding the item to equip.
    /// </summary>
    [Key(1)]
    public int SlotIndex { get; set; }
}
