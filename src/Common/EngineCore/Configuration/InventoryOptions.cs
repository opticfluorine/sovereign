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

namespace Sovereign.EngineCore.Configuration;

/// <summary>
///     Configurable options for inventory and item management.
/// </summary>
public sealed class InventoryOptions
{
    /// <summary>
    ///     Maximum allowed distance in world units between player and item when picking up the item.
    /// </summary>
    public float MaxPickupDistance { get; set; } = 0.5f;

    /// <summary>
    ///     Maximum allowed distance in world units between player and item when dropping an item.
    /// </summary>
    public float MaxDropDistance { get; set; } = 4.0f;

    /// <summary>
    ///     Default number of inventory slots for new players.
    /// </summary>
    public int NewPlayerDefaultSlots { get; set; } = 64;
}