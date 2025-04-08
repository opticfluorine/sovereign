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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Manages movement of entities for the movement system.
/// </summary>
public class MovementManager
{
    private readonly MovementInternalController internalController;

    /// <summary>
    ///     Jump flags indexed by the corresponding Kinematics component index.
    /// </summary>
    private readonly List<bool> isJumping = new();

    private readonly KinematicsComponentCollection kinematics;

    /// <summary>
    ///     Cache of physics tags indexed by the corresponding Kinematics component index.
    /// </summary>
    private readonly List<bool> kinematicsComponentIndexPhysicsTags = new();

    private readonly ILogger<MovementManager> logger;
    private readonly IMovementNotifier movementNotifier;
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

    private readonly List<bool> physicsActiveFlags = new();

    private readonly PhysicsProcessor physicsProcessor;
    private readonly List<int> physicsUpdates = new();

    /// <summary>
    ///     Map from entity ID to sequence counts.
    /// </summary>
    private readonly Dictionary<ulong, byte> sequenceCountsByEntity = new();

    private readonly ISystemTimer systemTimer;
    private readonly NonBlockWorldSegmentIndexer worldSegmentIndexer;

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

    public MovementManager(KinematicsComponentCollection kinematics,
        ISystemTimer systemTimer, MovementInternalController internalController,
        OrientationComponentCollection orientations,
        PhysicsTagCollection physics, PhysicsProcessor physicsProcessor,
        ILogger<MovementManager> logger, NonBlockWorldSegmentIndexer worldSegmentIndexer,
        IMovementNotifier movementNotifier)
    {
        this.kinematics = kinematics;
        this.systemTimer = systemTimer;
        this.internalController = internalController;
        this.orientations = orientations;
        this.physicsProcessor = physicsProcessor;
        this.logger = logger;
        this.worldSegmentIndexer = worldSegmentIndexer;
        this.movementNotifier = movementNotifier;

        for (var i = 0; i < pendingChecks.Length; ++i)
            pendingChecks[i] = new List<PendingCheck>();

        kinematics.OnStartUpdates += OnStartUpdates;
        kinematics.OnBeginDirectAccess += UpdatePositions;
        physics.OnComponentAdded += OnPhysicsTagAdded;
        physics.OnComponentRemoved += OnPhysicsTagRemoved;
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
        if (!kinematics.TryGetValue(details.EntityId, out var posVel))
        {
            logger.LogError("Tried to move entity {EntityID:X} which has no Kinematics.", details.EntityId);
            return;
        }

        var velocity = details.RelativeVelocity * MovementConfiguration.DefaultBaseVelocity;
        kinematics.ModifyComponent(details.EntityId, ComponentOperation.SetVelocity,
            new Kinematics { Velocity = new Vector3(velocity, posVel.Velocity.Z) });
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
        SetOrientation(details.EntityId, new Vector2(details.Velocity.X, details.Velocity.Y));
    }

    /// <summary>
    ///     Starts a jump for the specified entity if the entity is not already in a jump.
    /// </summary>
    /// <param name="entityId"></param>
    public void HandleJump(ulong entityId)
    {
        if (!kinematics.TryGetIndexForEntity(entityId, out var index))
        {
            logger.LogError("No kinematics component index for entity {EntityId:X} when jumping.", entityId);
            return;
        }

        if (isJumping[index]) return;

        var posVel = kinematics[entityId];
        kinematics.ModifyComponent(entityId, ComponentOperation.SetVelocity, new Kinematics
        {
            Velocity = posVel.Velocity with { Z = PhysicsConstants.InitialJumpVelocity }
        });
        isJumping[index] = true;
    }

    /// <summary>
    ///     Teleports an entity to a new position.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="newPosition">New position.</param>
    public void HandleTeleport(ulong entityId, Vector3 newPosition)
    {
        if (!kinematics.TryGetIndexForEntity(entityId, out var componentIndex))
        {
            logger.LogError("Tried to teleport entity {EntityId:X} which has no Kinematics.", entityId);
            return;
        }

        // Update position and flag for physics checks in the next tick.
        // The physics checks will also ensure that authoritative move updates are sent out as needed.
        // A teleport notice is sent to fill in any clients which are not subscribed to authoritiative move
        // updates in the new world segment.
        internalController.NotifyTeleport(entityId, newPosition);
        kinematics.ModifyComponent(entityId, ComponentOperation.Set, new Kinematics
        {
            Position = newPosition,
            Velocity = Vector3.Zero
        });
        physicsActiveFlags[componentIndex] = true;
        movementNotifier.ScheduleEntity(entityId);
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
            internalController.Move(entityId, kinematicData.Position, kinematicData.Velocity);
        }

        ProcessChecks();
        movementNotifier.SendScheduled();
    }

    /// <summary>
    ///     Called when collision meshes are changed.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z coordinate of Z plane.</param>
    public void OnMeshUpdate(GridPosition segmentIndex, int z)
    {
        // Flag all physics-enabled entities in this world segment and the one above for active processing.
        for (var segZ = segmentIndex.Z; segZ < segmentIndex.Z + 2; segZ++)
        {
            var recheckIndex = segmentIndex with { Z = segZ };
            var entities = worldSegmentIndexer.GetEntitiesInWorldSegment(recheckIndex);
            foreach (var entityId in entities)
            {
                if (!kinematics.TryGetIndexForEntity(entityId, out var kinematicsIndex)) continue;
                physicsActiveFlags[kinematicsIndex] = true;
            }
        }
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

        // Ensure flags exist for all components.
        while (kinematicsComponentIndexPhysicsTags.Count <= kinematics.Components.Length)
        {
            kinematicsComponentIndexPhysicsTags.Add(false);
            physicsActiveFlags.Add(false);
            isJumping.Add(false);
        }

        // Forward propagation step.
        physicsUpdates.Clear();
        for (var i = 0; i < kinematics.ComponentCount; ++i)
        {
            if (!kinematics.TryGetEntityForIndex(i, out var entityId)) continue;

            if (componentList[i].Velocity != Vector3.Zero)
            {
                // Using + instead of += shows a ~ 6.6% speedup with Release builds in EcsBenchmark.
                componentList[i].Position = componentList[i].Position + delta * componentList[i].Velocity;
                modifiedIndices[directMods++] = i;

                // We always process physics for moving entities (if they have the Physics tag).
                if (kinematicsComponentIndexPhysicsTags[i]) physicsUpdates.Add(i);

                // If this is the server, make sure a move event is sent in the next update batch.
                // This also schedules an event in the following batch to ensure that an event is
                // also sent when motion stops.
                movementNotifier.ScheduleEntity(entityId);
            }
            else if (physicsActiveFlags[i])
            {
                // Even if the entity is not moving, we want to do physics processing if it is tagged for update.
                physicsUpdates.Add(i);
            }
        }

        // Physics update step.
        foreach (var i in physicsUpdates)
            if (kinematics.TryGetEntityForIndex(i, out var entityId))
            {
                physicsProcessor.DoPhysicsForEntity(i, entityId, out var isActive, out var isSupportedBelow);
                physicsActiveFlags[i] = isActive;
                if (isSupportedBelow) isJumping[i] = false;
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
            // Stop movement (except for gravity) and send a move event.
            var posVel = kinematics[check.EntityId];
            var newVel = posVel.Velocity with { X = 0.0f, Y = 0.0f };
            kinematics.ModifyComponent(check.EntityId, ComponentOperation.SetVelocity,
                new Kinematics { Velocity = newVel });
            internalController.Move(check.EntityId, posVel.Position, newVel);
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
    /// <param name="entityId">Entity ID.</param>
    /// <param name="velocity">Velocity in the XY plane.</param>
    private void SetOrientation(ulong entityId, Vector2 velocity)
    {
        // Special case, if velocity is zero in the xy plane, then do not change the orientation.
        if (velocity is { X: 0.0f, Y: 0.0f }) return;

        // Find the angle of movement in the orthogonal space of the screen.
        // Note that Math.Atan2 properly handles the boundaries of the quadrants for us.
        var theta = (float)Math.Atan2(velocity.Y, velocity.X);

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
    ///     Called when a physics tag is added to an entity.
    /// </summary>
    /// <param name="entityId">Entity.</param>
    /// <param name="_">Unused.</param>
    /// <param name="__">Unused.</param>
    private void OnPhysicsTagAdded(ulong entityId, bool _, bool __)
    {
        if (!kinematics.TryGetIndexForEntity(entityId, out var index))
        {
            logger.LogError("No kinematics component index for entity {EntityId:X} when adding physics tag.", entityId);
            return;
        }

        // Ensure cache is large enough to hold the new component index.
        if (kinematicsComponentIndexPhysicsTags.Count <= index)
        {
            physicsUpdates.EnsureCapacity(kinematicsComponentIndexPhysicsTags.Count);
        }

        // Array capacity is ensured by the earlier call to UpdatePositions
        // (bulk updates are done before component events are fired, see BaseComponentCollection).
        kinematicsComponentIndexPhysicsTags[index] = true;
        physicsActiveFlags[index] = true;
    }

    /// <summary>
    ///     Called when a physics tag is removed from an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="_">Unused.</param>
    private void OnPhysicsTagRemoved(ulong entityId, bool _)
    {
        if (!kinematics.TryGetIndexForEntity(entityId, out var index))
        {
            logger.LogError("No kinematics component index for entity {EntityId:X} when removing physics tag.",
                entityId);
            return;
        }

        if (kinematicsComponentIndexPhysicsTags.Count <= index)
        {
            logger.LogError("Kinematics component index out of bounds for {EntityId:X}.", entityId);
            return;
        }

        kinematicsComponentIndexPhysicsTags[index] = false;
        physicsActiveFlags[index] = false;
        isJumping[index] = false;
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