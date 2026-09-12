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
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Details of an event requesting that a player unequip an equipped item into an inventory slot.
/// </summary>
[MessagePackObject]
public class UnequipEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID of the player performing the unequip. Overwritten by the server with the
    ///     authenticated player's entity ID when received over the network.
    /// </summary>
    [Key(0)]
    public ulong PlayerEntityId { get; set; }

    /// <summary>
    ///     Equipment type to unequip.
    /// </summary>
    [Key(1)]
    public EquipmentType EquipmentType { get; set; }

    /// <summary>
    ///     Index of the inventory slot to which the equipped item should be moved.
    /// </summary>
    [Key(2)]
    public int TargetSlotIndex { get; set; }
}
