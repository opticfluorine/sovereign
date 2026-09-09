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

using Sovereign.ClientCore.Rendering.Scenes.Game.Gui;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Handles mouse click interactions with entities.
/// </summary>
public sealed class EntityClickHandler(
    EntityTypeComponentCollection entityTypes,
    NpcFlagsComponentCollection npcFlags,
    ItemContextGui itemContextGui,
    IInventoryServices inventoryServices,
    ClientStateServices clientStateServices,
    IInventoryController inventoryController,
    ClientStateController stateController,
    IEventSender eventSender,
    UseRangeComponentCollection useRanges)
{
    /// <summary>
    ///     Called when an entity is clicked in game.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="button">Mouse button.</param>
    public void OnEntityClicked(ulong entityId, MouseButton button)
    {
        if (button == MouseButton.Left && TryUseSelectedTool(entityId)) return;

        if (!entityTypes.TryGetValue(entityId, out var entityType)) return;

        switch (entityType)
        {
            case EntityType.Item:
                OnItemClicked(entityId, button);
                break;

            case EntityType.Npc:
                OnNpcClicked(entityId, button);
                break;
        }
    }

    /// <summary>
    ///     Attempts to use the item selected in the hotbar as a tool on the clicked entity. Only
    ///     succeeds if the selected slot holds an item with a UseRange component.
    /// </summary>
    /// <param name="targetEntityId">Entity ID of the clicked entity.</param>
    /// <returns>true if the use was initiated, false to fall through to other click handling.</returns>
    private bool TryUseSelectedTool(ulong targetEntityId)
    {
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId)) return false;

        var itemId = inventoryServices.GetItem(playerId, clientStateServices.GetSelectedHotbarSlot());
        if (itemId == 0) return false;

        if (!useRanges.TryGetValue(itemId, out _)) return false;

        inventoryController.UseItem(eventSender, playerId, itemId, targetEntityId);
        return true;
    }

    /// <summary>
    ///     Called when an NPC is clicked.
    /// </summary>
    /// <param name="npcId">NPC entity ID.</param>
    /// <param name="button">Mouse button.</param>
    private void OnNpcClicked(ulong npcId, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Right:
                // Right-clicking an NPC with an inventory opens its inventory in the secondary window,
                // but only if the NPC has the Chest flag set.
                if (npcFlags.TryGetValue(npcId, out var flags) &&
                    (flags & NpcFlag.Chest) > 0 &&
                    inventoryServices.GetSlotCount(npcId) > 0)
                    stateController.SetSecondaryInventoryEntity(eventSender, npcId);
                break;
        }
    }

    /// <summary>
    ///     Called when an item is clicked.
    /// </summary>
    /// <param name="itemId">Item entity ID.</param>
    /// <param name="button">Mouse button.</param>
    private void OnItemClicked(ulong itemId, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                PickUpItem(itemId);
                break;

            case MouseButton.Right:
                OpenItemContextMenu(itemId);
                break;
        }
    }

    /// <summary>
    ///     Picks up an item if in range.
    /// </summary>
    /// <param name="itemId">Item entity ID.</param>
    private void PickUpItem(ulong itemId)
    {
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId) ||
            !inventoryServices.CanPickUp(playerId, itemId)) return;

        inventoryController.PickUp(eventSender, playerId, itemId);
    }

    /// <summary>
    ///     Opens the context menu for the given item.
    /// </summary>
    /// <param name="itemId">Item ID.</param>
    private void OpenItemContextMenu(ulong itemId)
    {
        itemContextGui.Open(itemId);
    }
}