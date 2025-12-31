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

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Public service methods exposed by the Inventory system.
/// </summary>
public interface IInventoryServices
{
    /// <summary>
    ///     Checks whether an entity is allowed to pick up the given item.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <returns>true if allowed, false otherwise.</returns>
    bool CanPickUp(ulong entityId, ulong itemId);
}

/// <summary>
///     Implementation of IInventoryServices.
/// </summary>
internal sealed class InventoryServices(InventoryManager inventoryManager) : IInventoryServices
{
    public bool CanPickUp(ulong entityId, ulong itemId)
    {
        return inventoryManager.IsPickUpAllowed(entityId, itemId);
    }
}