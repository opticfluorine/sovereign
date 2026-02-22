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

using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Inventory;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Mouse click handler for inventory-related actions.
/// </summary>
public sealed class InventoryClickHandler(
    IPerspectiveServices perspectiveServices,
    CameraServices cameraServices,
    IEventSender eventSender,
    IInventoryController inventoryController,
    ClientStateServices stateServices,
    ILogger<InventoryClickHandler> logger)
{
    /// <summary>
    ///     Drops the currently selected item at the mouse position if it is in range.
    /// </summary>
    /// <param name="slotIndex">Slot index.</param>
    /// <param name="snapToGrid">Whether to snap the dropped item to the block grid.</param>
    public void DropSelectedItem(int slotIndex, bool snapToGrid)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("No player ID.");
            return;
        }

        var mousePos = cameraServices.GetMousePositionWorldCoordinates();
        if (!perspectiveServices.TryGetHighestVisibleCoveringBlock(mousePos, out var blockId, out var faceType,
                out var blockPos)) return;

        if (faceType != PerspectiveEntityType.BlockTopFace) return;
        if (snapToGrid) blockPos = blockPos.Floor();
        else
            // Shift down by 1 unit to account for mouse cursor offset (otherwise the item will appear to jump up)
            blockPos -= new Vector3(0.0f, 1.0f, 0.0f);

        // No need to do a range check here - the inventory system will do its own.
        inventoryController.Drop(eventSender, playerId, slotIndex, blockPos);
    }
}