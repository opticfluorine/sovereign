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

using System;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     World editor tool handler for NPC placement and removal.
/// </summary>
internal class NpcToolHandler(
    CameraServices cameraServices,
    PerspectiveServices perspectiveServices,
    ClientWorldEditInternalController internalController,
    IEventSender eventSender,
    EntityTypeComponentCollection entityTypes)
    : IWorldEditToolHandler
{
    public void ProcessDraw()
    {
        // Get highest overlapping visible block at the hovered position (hx, hy, hz).
        // If a top face is found for a block at (bx, by, bz), place NPC at (hx, hy, bz).
        // If a front face is found for a block at (bx, by, bz), place NPC at (hx, by, hz).
        // If nothing is found, place NPC at (hx, hy, hz).
    }

    public void ProcessErase()
    {
        var hoveredPos = cameraServices.GetMousePositionWorldCoordinates();
        if (perspectiveServices.TryGetHighestCoveringEntity(hoveredPos, out var entityId))
            if (entityTypes.TryGetValue(entityId, out var entityType) && entityType == EntityType.Npc)
                internalController.RemoveNpc(eventSender, entityId);
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}