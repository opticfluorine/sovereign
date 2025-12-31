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
    ItemContextGui itemContextGui,
    IInventoryServices inventoryServices,
    ClientStateServices clientStateServices,
    IInventoryController inventoryController,
    IEventSender eventSender)
{
    /// <summary>
    ///     Called when an entity is clicked in game.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="button">Mouse button.</param>
    public void OnEntityClicked(ulong entityId, MouseButton button)
    {
        if (!entityTypes.TryGetValue(entityId, out var entityType)) return;

        switch (entityType)
        {
            case EntityType.Item:
                OnItemClicked(entityId, button);
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

        inventoryController.PickUp(eventSender, itemId);
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