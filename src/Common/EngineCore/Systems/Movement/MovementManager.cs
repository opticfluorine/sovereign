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

using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
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
    private readonly MovingComponentIndexer movingComponents;

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

    private readonly PositionComponentCollection positions;

    /// <summary>
    ///     Map from entity ID to sequence counts.
    /// </summary>
    private readonly Dictionary<ulong, byte> sequenceCountsByEntity = new();

    private readonly ISystemTimer systemTimer;
    private readonly VelocityComponentCollection velocities;

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

    public MovementManager(MovingComponentIndexer movingComponents, PositionComponentCollection positions,
        VelocityComponentCollection velocities, ISystemTimer systemTimer, MovementInternalController internalController,
        IEventSender eventSender)
    {
        this.movingComponents = movingComponents;
        this.positions = positions;
        this.velocities = velocities;
        this.systemTimer = systemTimer;
        this.internalController = internalController;
        this.eventSender = eventSender;

        for (var i = 0; i < pendingChecks.Length; ++i)
            pendingChecks[i] = new List<PendingCheck>();
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
        velocities.ModifyComponent(details.EntityId, ComponentOperation.Set, velocity);
        sequenceCountsByEntity[details.EntityId] = details.Sequence;
        pendingMoveEvents.Enqueue(details.EntityId);

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
    public void HandleeAuthoritativeMove(MoveEventDetails details)
    {
        // Set position and velocity.
        positions.ModifyComponent(details.EntityId, ComponentOperation.Set, details.Position);
        velocities.ModifyComponent(details.EntityId, ComponentOperation.Set, details.Velocity);
    }

    /// <summary>
    ///     Called at the start of a new tick.
    /// </summary>
    public void HandleTick()
    {
        if (lastUpdateSystemTime == 0) lastUpdateSystemTime = systemTimer.GetTime();
        UpdatePositions();

        // Send move events for any newly processed move requests.
        while (pendingMoveEvents.TryDequeue(out var entityId))
            internalController.Move(eventSender, entityId, positions[entityId], velocities[entityId],
                sequenceCountsByEntity[entityId]);

        ProcessChecks();
    }

    /// <summary>
    ///     Updates all positions at the beginning of a tick.
    /// </summary>
    private void UpdatePositions()
    {
        var currentSystemTime = systemTimer.GetTime();
        var delta = (currentSystemTime - lastUpdateSystemTime) * UnitConversions.UsToS;
        foreach (var entityId in movingComponents.MovingEntities)
        {
            var velocity = velocities.GetComponentForEntity(entityId);
            if (!velocity.HasValue) continue;

            var deltaPosition = delta * velocity.Value;
            positions.ModifyComponent(entityId, ComponentOperation.Add, deltaPosition);
        }

        lastUpdateSystemTime = currentSystemTime;
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
            velocities.ModifyComponent(check.EntityId, ComponentOperation.Set, Vector3.Zero);
            internalController.Move(eventSender, check.EntityId, positions[check.EntityId],
                Vector3.Zero, check.SequenceCount);
        }

        // Reset the check list so that it can be populated with new checks for move requests arriving this tick.
        pendingChecks[currentTickIndex].Clear();
        checkTableReady = true;

        // Process any move requests that arrived this tick before the pending checks were processed.
        while (checkTableReady && pendingRequests.TryDequeue(out var details)) HandleRequestMove(details);
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