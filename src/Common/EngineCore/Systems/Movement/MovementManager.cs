// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Manages movement of entities for the movement system.
/// </summary>
public class MovementManager
{
    private readonly IEventSender eventSender;
    private readonly MovementInternalController internalController;

    private readonly KinematicComponentCollection kinematics;
    private readonly OrientationComponentCollection orientations;

    /// <summary>
    ///     Circular buffer of lists of pending checks per tick.
    /// </summary>
    private readonly List<PendingCheck>[] pendingChecks =
        new List<PendingCheck>[MovementConfiguration.DefaultMovementLengthTicks];

    /// <summary>
    ///     Queue of entities needing move events sent.
    /// </summary>
    private readonly Queue<ulong> pendingMoveEvents = new();

    /// <summary>
    ///     Queue of move requests received at beginning of tick before the check table is updated.
    /// </summary>
    private readonly Queue<RequestMoveEventDetails> pendingRequests = new();

    /// <summary>
    ///     Map from entity ID to sequence counts.
    /// </summary>
    private readonly Dictionary<ulong, byte> sequenceCountsByEntity = new();

    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Flag indicating whether the pending check table has been updated for the current tick.
    /// </summary>
    private bool checkTableReady;

    /// <summary>
    ///     Index into pendingChecks for the current tick.
    ///     Next tick will be (currentTickIndex + 1) % (movement interval).
    /// </summary>
    private int currentTickIndex;

    /// <summary>
    ///     System time of last position update. Used for interpolation.
    /// </summary>
    private ulong lastUpdateSystemTime;

    public MovementManager(KinematicComponentCollection kinematics,
        ISystemTimer systemTimer, MovementInternalController internalController,
        IEventSender eventSender, OrientationComponentCollection orientations)
    {
        this.kinematics = kinematics;
        this.systemTimer = systemTimer;
        this.internalController = internalController;
        this.eventSender = eventSender;
        this.orientations = orientations;

        for (var i = 0; i < pendingChecks.Length; ++i)
            pendingChecks[i] = new List<PendingCheck>();

        kinematics.OnStartUpdates += OnStartUpdates;
        kinematics.OnBeginDirectAccess += UpdatePositions;
    }

    /// <summary>
    ///     Called when movement is requested.
    /// </summary>
    /// <param name="details">Details.</param>
    public void HandleRequestMove(RequestMoveEventDetails details)
    {
        // If the pending check table hasn't been updated for the current tick yet, defer processing
        // until it has been updated.
        if (!checkTableReady)
        {
            pendingRequests.Enqueue(details);
            return;
        }

        // Set velocity and update records.
        var velocity = details.RelativeVelocity * MovementConfiguration.DefaultBaseVelocity;
        kinematics.ModifyComponent(details.EntityId, ComponentOperation.SetVelocity,
            new Kinematics { Velocity = velocity });
        sequenceCountsByEntity[details.EntityId] = details.Sequence;
        pendingMoveEvents.Enqueue(details.EntityId);

        SetOrientation(details.EntityId, details.RelativeVelocity);

        // Schedule a check at the end of the movement interval.
        pendingChecks[currentTickIndex].Add(new PendingCheck
        {
            EntityId = details.EntityId,
            SequenceCount = details.Sequence
        });
    }


    /// <summary>
    ///     Called when an authoritative movement event is received from the server.
    /// </summary>
    /// <param name="details">Details.</param>
    public void HandleAuthoritativeMove(MoveEventDetails details)
    {
        // Set position and velocity.
        kinematics.ModifyComponent(details.EntityId, ComponentOperation.Set, new Kinematics
        {
            Position = details.Position,
            Velocity = details.Velocity
        });
        SetOrientation(details.EntityId, details.Velocity);
    }

    /// <summary>
    ///     Called at the start of a new tick.
    /// </summary>
    public void HandleTick()
    {
        // Send move events for any newly processed move requests.
        while (pendingMoveEvents.TryDequeue(out var entityId))
        {
            var kinematicData = kinematics[entityId];
            internalController.Move(eventSender, entityId, kinematicData.Position, kinematicData.Velocity,
                sequenceCountsByEntity[entityId]);
        }

        ProcessChecks();
    }

    /// <summary>
    ///     Updates all positions at the beginning of a tick.
    /// </summary>
    private int UpdatePositions(int[] modifiedIndices)
    {
        if (lastUpdateSystemTime == 0) lastUpdateSystemTime = systemTimer.GetTime();
        var currentSystemTime = systemTimer.GetTime();
        var delta = (currentSystemTime - lastUpdateSystemTime) * UnitConversions.UsToS;

        var componentList = kinematics.Components;
        var directMods = 0;
        for (var i = 0; i < kinematics.ComponentCount; ++i)
            if (componentList[i].Velocity != Vector3.Zero)
            {
                // Using + instead of += shows a ~ 6.6% speedup with Release builds in EcsBenchmark.
                componentList[i].Position = componentList[i].Position + delta * componentList[i].Velocity;
                modifiedIndices[directMods++] = i;
            }

        lastUpdateSystemTime = currentSystemTime;
        return directMods;
    }

    /// <summary>
    ///     Processes pending checks for the latest tick.
    /// </summary>
    private void ProcessChecks()
    {
        // Advance the check buffer and process all checks.
        currentTickIndex = (currentTickIndex + 1) % pendingChecks.Length;
        foreach (var check in pendingChecks[currentTickIndex])
        {
            // Process check.
            // Skip if superseded by a more recent move request.
            if (check.SequenceCount != sequenceCountsByEntity[check.EntityId])
                continue;

            // Check is current and no newer move request has been received.
            // Stop movement and send a move event.
            kinematics.ModifyComponent(check.EntityId, ComponentOperation.SetVelocity,
                new Kinematics { Velocity = Vector3.Zero });
            internalController.Move(eventSender, check.EntityId, kinematics[check.EntityId].Position,
                Vector3.Zero, check.SequenceCount);
        }

        // Reset the check list so that it can be populated with new checks for move requests arriving this tick.
        pendingChecks[currentTickIndex].Clear();
        checkTableReady = true;

        // Process any move requests that arrived this tick before the pending checks were processed.
        while (checkTableReady && pendingRequests.TryDequeue(out var details)) HandleRequestMove(details);
    }

    /// <summary>
    ///     Called when velocity updates begin between ticks.
    /// </summary>
    private void OnStartUpdates()
    {
        // Pause request processing until checks are processed again.
        checkTableReady = false;
    }

    /// <summary>
    ///     Sets the orientation of an entity based on its velocity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="velocity"></param>
    private void SetOrientation(ulong entityId, Vector3 velocity)
    {
        // Special case, if velocity is zero then do not change the orientation.
        if (velocity.Equals(Vector3.Zero)) return;

        // Find the angle of movement in the orthogonal space of the screen.
        // Project y and z together to account for z-oriented motion being projected onto the y axis for rendering.
        // Note that Math.Atan2 properly handles the boundaries of the quadrants for us.
        var projY = velocity.Y + velocity.Z;
        var theta = (float)Math.Atan2(projY, velocity.X);

        // Map the angle onto the defined orientations.
        // Rotate by pi/8 radians so that the first bin starts at 0.
        // Rotate by another 2pi radians to bring the quadrant 3 and 4 angles to positive
        // Then bins begin at angles of n(pi/4) for integer n (modulo 8), offset by two from the enum ordering.
        const float invBinWidth = 1.0f / (0.25f * (float)Math.PI);
        const int orientationCount = 8; // Eight defined orientations
        const int orientationOffset = 2; // theta=0 is East, so offset by two to give South as the first in
        var adjTheta = theta + 2.125f * (float)Math.PI;
        var orientation = (Orientation)((int)(adjTheta * invBinWidth + orientationOffset) % orientationCount);

        orientations.AddOrUpdateComponent(entityId, orientation);
    }

    /// <summary>
    ///     Pending check info.
    /// </summary>
    private struct PendingCheck
    {
        /// <summary>
        ///     Affected entity ID.
        /// </summary>
        public ulong EntityId { get; set; }

        /// <summary>
        ///     Sequence count associated with this check.
        /// </summary>
        public byte SequenceCount { get; set; }
    }
}