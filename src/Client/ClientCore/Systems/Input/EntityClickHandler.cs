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
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Block;
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
    UseRangeComponentCollection useRanges,
    ItemUseComponentCollection itemUses,
    IPerspectiveServices perspectiveServices,
    CameraServices cameraServices,
    BlockPositionComponentCollection blockPositions,
    IBlockServices blockServices)
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

        // Tools that only target empty blocks select the empty cell adjacent to the clicked block face.
        if (itemUses.TryGetValue(itemId, out var itemUse) && itemUse == ItemUse.EmptyBlock)
            return TryUseOnEmptyBlock(playerId, itemId);

        inventoryController.UseItem(eventSender, playerId, itemId, targetEntityId,
            ResolveFace(targetEntityId));
        return true;
    }

    /// <summary>
    ///     Attempts to use a tool on the empty cell adjacent to the clicked block face.
    /// </summary>
    /// <param name="playerId">Player entity ID.</param>
    /// <param name="itemId">Tool item entity ID.</param>
    /// <returns>true if the use was initiated, false to fall through to other click handling.</returns>
    private bool TryUseOnEmptyBlock(ulong playerId, ulong itemId)
    {
        var mousePos = cameraServices.GetMousePositionWorldCoordinates();
        if (!perspectiveServices.TryGetHighestVisibleCoveringBlock(mousePos, out var blockId, out var faceType,
                out _)) return false;

        var face = MapFace(faceType);
        if (!blockPositions.TryGetValue(blockId, out var blockPosition)) return false;
        if (!ItemUseFaceUtil.TryGetAdjacentCell(blockPosition, face, out var cell)) return false;
        if (blockServices.BlockExistsAtPosition(cell)) return false;

        inventoryController.UseItem(eventSender, playerId, itemId, blockId, face);
        return true;
    }

    /// <summary>
    ///     Resolves the block face clicked by the mouse for the given target entity. Falls back to
    ///     <see cref="ItemUseFace.None" /> if the click cannot be resolved to the target, leaving
    ///     validation to the server.
    /// </summary>
    /// <param name="targetEntityId">Entity ID of the clicked entity.</param>
    /// <returns>Resolved block face, or <see cref="ItemUseFace.None" />.</returns>
    private ItemUseFace ResolveFace(ulong targetEntityId)
    {
        if (!EntityUtil.IsBlockEntity(targetEntityId)) return ItemUseFace.None;

        var mousePos = cameraServices.GetMousePositionWorldCoordinates();
        if (!perspectiveServices.TryGetHighestVisibleCoveringBlock(mousePos, out var blockId, out var faceType,
                out _) || blockId != targetEntityId) return ItemUseFace.None;

        return MapFace(faceType);
    }

    /// <summary>
    ///     Maps a perspective entity type to the corresponding item use face.
    /// </summary>
    /// <param name="faceType">Perspective entity type.</param>
    /// <returns>Item use face, or <see cref="ItemUseFace.None" /> if the type is not a block face.</returns>
    private static ItemUseFace MapFace(PerspectiveEntityType faceType)
    {
        return faceType switch
        {
            PerspectiveEntityType.BlockFrontFace => ItemUseFace.BlockFrontFace,
            PerspectiveEntityType.BlockTopFace => ItemUseFace.BlockTopFace,
            _ => ItemUseFace.None
        };
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
