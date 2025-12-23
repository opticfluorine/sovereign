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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     World editor tool handler for item placement and removal.
/// </summary>
public class ItemToolHandler(
    CameraServices cameraServices,
    IPerspectiveServices perspectiveServices,
    ClientWorldEditState state,
    BoundingBoxComponentCollection boundingBoxes,
    ILogger<ItemToolHandler> logger,
    ClientWorldEditInternalController internalController,
    IEventSender eventSender,
    LoggingUtil loggingUtil,
    EntityTypeComponentCollection entityTypes) : IWorldEditToolHandler
{
    private bool actionAlreadyPerformed;

    public void ProcessDraw()
    {
        if (actionAlreadyPerformed) return;
        actionAlreadyPerformed = true;

        var position = cameraServices.GetMousePositionWorldCoordinates();
        if (perspectiveServices.TryGetHighestVisibleCoveringBlock(position, out _,
                out var entityType, out var positionOnBlock))
        {
            // If the entity has physics, translate the new item out of the hovered block.
            // It may still intersect other blocks, however the physics engine will handle that after it is added.
            var zShift = 0.0f;
            if (entityType == PerspectiveEntityType.BlockFrontFace &&
                boundingBoxes.TryGetValue(state.ItemTemplateId, out var boundingBox))
                zShift = boundingBox.Size.Z;

            logger.LogDebug("Apply Z shift of {ZShift}.", zShift);
            position = positionOnBlock with { Z = positionOnBlock.Z - zShift };
        }

        if (state.SnapToGrid)
            position = new Vector3((float)Math.Floor(position.X), (float)Math.Floor(position.Y),
                (float)Math.Floor(position.Z));

        logger.LogDebug("Add item {ItemName} at {Position}.", loggingUtil.FormatEntity(state.ItemTemplateId), position);
        internalController.AddNonBlock(eventSender, position, state.ItemTemplateId);
    }

    public void ProcessErase()
    {
        if (actionAlreadyPerformed) return;
        actionAlreadyPerformed = true;

        var hoveredPos = cameraServices.GetMousePositionWorldCoordinates();
        if (perspectiveServices.TryGetHighestCoveringEntity(hoveredPos, out var entityId))
            if (entityTypes.TryGetValue(entityId, out var entityType) && entityType == EntityType.Item)
                internalController.RemoveNonBlock(eventSender, entityId);
    }

    public void Reset()
    {
        actionAlreadyPerformed = false;
    }
}