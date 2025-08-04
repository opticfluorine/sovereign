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

using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Interaction;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Ranges;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Responsible for processing entity interactions as the result of player input.
/// </summary>
public sealed class PlayerInteractionHandler(
    ClientStateServices stateServices,
    ILogger<PlayerInteractionHandler> logger,
    IEventSender eventSender,
    InteractionController interactionController,
    KinematicsComponentCollection kinematics,
    OrientationComponentCollection orientations,
    NonBlockWorldSegmentIndexer indexer,
    WorldSegmentResolver resolver,
    BoundingBoxComponentCollection boundingBoxes)
{
    private const float MaxRange = 0.1f;

    private readonly List<GridPosition> searchSegments = new(2);

    /// <summary>
    ///     Processes a player interaction at the client.
    /// </summary>
    public void Interact()
    {
        if (!stateServices.TryGetSelectedPlayer(out var sourceEntityId))
        {
            logger.LogError("Tried interaction without player selected.");
            return;
        }

        if (!TryIdentifyTarget(sourceEntityId, out var targetEntityId)) return;

        interactionController.Interact(eventSender, sourceEntityId, targetEntityId);
    }

    /// <summary>
    ///     Determines if the player is targeting an entity for interaction.
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID. Only meaningful if the method returns true.</param>
    /// <returns>true if a targeted entity is found, false otherwise.</returns>
    private bool TryIdentifyTarget(ulong sourceEntityId, out ulong targetEntityId)
    {
        targetEntityId = 0;

        // Locate the nearest non-block entity in the direction the player is facing that is also within
        // the allowed interaction range.
        if (!kinematics.TryGetValue(sourceEntityId, out var posVel) ||
            !orientations.TryGetValue(sourceEntityId, out var orientation) ||
            !boundingBoxes.TryGetValue(sourceEntityId, out var boundingBox))
        {
            logger.LogError("Player is missing required data.");
            return false;
        }

        boundingBox = boundingBox.Translate(posVel.Position);

        // Determine which world segments need to be checked.
        var aimVector = OrientationUtil.GetUnitVector(orientation);
        var basePos = boundingBox.FindInterceptFromCenter(aimVector);

        var maxPos = basePos + aimVector * MaxRange;
        var baseSegment = resolver.GetWorldSegmentForPosition(basePos);
        var farSegment = resolver.GetWorldSegmentForPosition(maxPos);
        searchSegments.Clear();
        searchSegments.Add(baseSegment);
        if (farSegment != baseSegment) searchSegments.Add(farSegment);

        // Search non-block entities.
        var nearest = (D2: float.MaxValue, EntityId: 0UL);
        foreach (var segmentIndex in searchSegments)
        foreach (var entityId in indexer.GetEntitiesInWorldSegment(segmentIndex))
        {
            if (entityId == sourceEntityId) continue;
            if (!kinematics.TryGetValue(entityId, out var tgtPosVel) ||
                !boundingBoxes.TryGetValue(entityId, out var targetBoundingBox)) continue;
            targetBoundingBox = targetBoundingBox.Translate(tgtPosVel.Position);

            // Exclude distant entities based on the distance between their bounding boxes where the boxes
            // intercept a line drawn between the target center and the player face. This is roughly the
            // "closest approach" of the axis-aligned bounding boxes for the two entities.
            var targetCenter = tgtPosVel.Position + 0.5f * targetBoundingBox.Size;
            var targetAimVector = basePos - targetCenter;
            var targetPos = targetBoundingBox.FindInterceptFromCenter(targetAimVector);

            // Exclude if z ranges do not overlap.
            if (!RangeUtil.RangesIntersect(boundingBox.ZRange, targetBoundingBox.ZRange)) continue;

            // Rank by distance in xy plane (with cutoff).
            var delta = targetPos - basePos;
            if (Vector3.Dot(delta, aimVector) < 0.0f) continue; // exclude entities behind the player
            var delta2 = delta * delta;
            var d2 = delta2.X + delta2.Y;
            if (d2 >= MaxRange * MaxRange) continue; // exclude entities that are out of range

            if (d2 < nearest.D2) nearest = (d2, entityId); // keep only if closer than any other entity considered
        }

        if (nearest.D2 > MaxRange * MaxRange) return false;
        targetEntityId = nearest.EntityId;
        return true;
    }
}