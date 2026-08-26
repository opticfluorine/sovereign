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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Determines whether an actor entity is permitted to modify an entity's inventory.
/// </summary>
public sealed class InventoryPermissionService
{
    private readonly NpcFlagsComponentCollection npcFlags;

    public InventoryPermissionService(NpcFlagsComponentCollection npcFlags)
    {
        this.npcFlags = npcFlags;
    }

    /// <summary>
    ///     Determines whether the given actor is permitted to swap items between the two
    ///     specified inventories. One inventory must be the actor's own, and the other must
    ///     either also be the actor's own or have the Chest NpcFlag set.
    /// </summary>
    /// <param name="actorEntityId">Actor entity ID.</param>
    /// <param name="inventory0Id">Entity ID that owns the first inventory.</param>
    /// <param name="inventory1Id">Entity ID that owns the second inventory.</param>
    /// <returns>true if the actor may swap between these inventories, false otherwise.</returns>
    public bool IsSwapPermitted(ulong actorEntityId, ulong inventory0Id, ulong inventory1Id)
    {
        if (inventory0Id == actorEntityId && inventory1Id == actorEntityId)
            return true;

        if (inventory0Id == actorEntityId)
            return HasChestFlag(inventory1Id);

        if (inventory1Id == actorEntityId)
            return HasChestFlag(inventory0Id);

        return false;
    }

    private bool HasChestFlag(ulong entityId)
    {
        return npcFlags.TryGetValue(entityId, out var flags) &&
               (flags & NpcFlag.Chest) > 0;
    }
}
