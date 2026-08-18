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
///     Determines whether an actor entity is permitted to modify an entity's inventory.
/// </summary>
public sealed class InventoryPermissionService
{
    /// <summary>
    ///     Determines whether the given actor is permitted to modify the given inventory.
    /// </summary>
    /// <param name="actorEntityId">Actor entity ID.</param>
    /// <param name="inventoryEntityId">Entity ID that owns the inventory.</param>
    /// <returns>true if the actor may modify the inventory, false otherwise.</returns>
    public bool CanModify(ulong actorEntityId, ulong inventoryEntityId)
    {
        return actorEntityId == inventoryEntityId;
    }
}
