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
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Detects and handles collisions between physics-enabled entities and blocks.
/// </summary>
public class PhysicsProcessor
{
    /// <summary>
    ///     Maximum number of collision passes per entity before collision resolution is abandoned.
    /// </summary>
    private const int MaxCollisionPasses = 4;

    /// <summary>
    ///     Cutoff for equality testing in surface contact testing.
    /// </summary>
    private const float ContactTestEpsilon = 1e-6f;

    /// <summary>
    ///     Change in velocity applied per tick due to gravity.
    /// </summary>
    private const float GravityVelocityStep =
        PhysicsConstants.GravityAcceleration * EngineConstants.TickIntervalUs * UnitConversions.UsToS;

    /// <summary>
    ///     Number of ticks to wait after a world segment is loaded before starting physics processing.
    /// </summary>
    private const ulong PostLoadProcessingDelayTicks = 3;

    /// <summary>
    ///     "Radius" in world segments of non-block collision mesh search.
    /// </summary>
    private const int NonBlockSearchRadius = 1;

    private readonly List<List<BoundingBox>> activeMeshes = [];
    private readonly HashSet<GridPosition> activeWorldSegments = new();
    private readonly BoundingBoxComponentCollection boundingBoxes;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ILogger<PhysicsProcessor> logger;
    private readonly CollisionMeshManager meshManager;
    private readonly NonBlockCollisionMeshes nonBlockCollisionMeshes;
    private readonly Queue<Tuple<ulong, GridPosition>> pendingWorldSegments = new();
    private readonly WorldSegmentResolver resolver;

    private ulong tickCount;

    public PhysicsProcessor(CollisionMeshManager meshManager, KinematicsComponentCollection kinematics,
        BoundingBoxComponentCollection boundingBoxes, WorldSegmentResolver resolver, ILogger<PhysicsProcessor> logger,
        NonBlockCollisionMeshes nonBlockCollisionMeshes)
    {
        this.meshManager = meshManager;
        this.kinematics = kinematics;
        this.boundingBoxes = boundingBoxes;
        this.resolver = resolver;
        this.logger = logger;
        this.nonBlockCollisionMeshes = nonBlockCollisionMeshes;
    }

    /// <summary>
    ///     Called when a world segment has been loaded.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void OnWorldSegmentLoaded(GridPosition segmentIndex)
    {
        pendingWorldSegments.Enqueue(Tuple.Create(tickCount + PostLoadProcessingDelayTicks, segmentIndex));
    }

    /// <summary>
    ///     Called when a world segment has been unloaded.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void OnWorldSegmentUnloaded(GridPosition segmentIndex)
    {
        activeWorldSegments.Remove(segmentIndex);
    }

    /// <summary>
    ///     Called at the start of a new tick.
    /// </summary>
    public void OnTick()
    {
        tickCount++;

        while (pendingWorldSegments.TryPeek(out var segmentInfo) && segmentInfo.Item1 <= tickCount)
        {
            activeWorldSegments.Add(segmentInfo.Item2);
            pendingWorldSegments.Dequeue();
        }
    }

    /// <summary>
    ///     Checks for and handles collisions.
    /// </summary>
    /// <param name="kinematicsIndex">Kinematics component direct index.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isActive">Set to true if physics processing should continue in the next tick.</param>
    /// <param name="isSupportedBelow">Set to true if the entity is supported from below by a surface.</param>
    public void DoPhysicsForEntity(int kinematicsIndex, ulong entityId, out bool isActive, out bool isSupportedBelow)
    {
        isActive = false;
        isSupportedBelow = false;

        var segmentIndex = resolver.GetWorldSegmentForPosition(kinematics.Components[kinematicsIndex].Position);
        if (!activeWorldSegments.Contains(segmentIndex))
        {
            // Entity is in a world segment that is not yet loaded. Revisit for processing at a later tick.
            isActive = true;
            return;
        }

        if (!boundingBoxes.TryGetValue(entityId, out var sourceMesh))
        {
            logger.LogError("No BoundingBox for entity {EntityID:X}.", entityId);
            return;
        }

        var passes = 0;
        var changed = true;
        while (changed && passes < MaxCollisionPasses)
        {
            passes++;

            var posVel = kinematics.Components[(ulong)kinematicsIndex];
            var entityMesh = sourceMesh.Translate(posVel.Position);
            SelectActiveMeshes(entityMesh);

            changed = HandleCollisions(kinematicsIndex, entityMesh, posVel, entityId, out isSupportedBelow);
        }

        if (changed)
            logger.LogWarning("Unresolved collision for entity {EntityId:X}.", entityId);

        if (!isSupportedBelow)
        {
            ApplyGravity(kinematicsIndex);
            isActive = true;
        }
    }

    /// <summary>
    ///     Selects the active block collision meshes that should be considered for the given bounds.
    /// </summary>
    /// <param name="entityMesh">Entity collision mesh.</param>
    private void SelectActiveMeshes(BoundingBox entityMesh)
    {
        activeMeshes.Clear();

        var entityStart = entityMesh.Position;
        var entityEnd = entityStart + entityMesh.Size;

        var firstSegment = resolver.GetWorldSegmentForPosition(entityStart);
        var lastSegment = resolver.GetWorldSegmentForPosition(entityEnd);

        // Include the z planes above and below the entity mesh in the search in order to
        // handle the surface contact edge case (necessary for gravity to work correctly).
        var firstZ = (int)Math.Floor(entityStart.Z) - 1;
        var lastZ = (int)Math.Ceiling(entityEnd.Z);

        for (var segX = firstSegment.X; segX <= lastSegment.X; ++segX)
        for (var segY = firstSegment.Y; segY <= lastSegment.Y; ++segY)
        for (var segZ = firstSegment.Z; segZ <= lastSegment.Z; ++segZ)
        {
            var segmentIndex = new GridPosition(segX, segY, segZ);
            var segmentRange = resolver.GetRangeForWorldSegment(segmentIndex);
            var segMinZ = Math.Max(firstZ, (int)segmentRange.Item1.Z);
            var segMaxZ = Math.Min(lastZ, (int)segmentRange.Item2.Z);
            for (var z = segMinZ; z <= segMaxZ; ++z)
                if (meshManager.TryGetMeshes(segmentIndex, z, out var meshes))
                    activeMeshes.Add(meshes);
        }
    }

    /// <summary>
    ///     Detects and handles collisions between the entity and the active meshes.
    /// </summary>
    /// <param name="kinematicsIndex">Kinematics component index.</param>
    /// <param name="entityMesh">Entity collision mesh.</param>
    /// <param name="posVel">Current position and velocity data for entity.</param>
    /// <param name="supportedBelow">true if the entity has a surface contact with a mesh below, false otherwise.</param>
    /// <returns>true if the entity was moved to resolve a collision, false otherwise.</returns>
    private bool HandleCollisions(int kinematicsIndex, BoundingBox entityMesh, Kinematics posVel, ulong entityId,
        out bool supportedBelow)
    {
        supportedBelow = false;

        // Block collisions.
        foreach (var layer in activeMeshes)
        foreach (var blockMesh in layer)
        {
            if (CheckSingleCollision(kinematicsIndex, entityMesh, posVel, ref supportedBelow, blockMesh)) return true;
        }

        // Non-block collisions.
        var centerIndex = resolver.GetWorldSegmentForPosition(posVel.Position);
        for (var i = centerIndex.X - NonBlockSearchRadius; i < centerIndex.X + NonBlockSearchRadius + 1; ++i)
        for (var j = centerIndex.Y - NonBlockSearchRadius; j < centerIndex.Y + NonBlockSearchRadius + 1; ++j)
        for (var k = centerIndex.Z - NonBlockSearchRadius; k < centerIndex.Z + NonBlockSearchRadius + 1; ++k)
        {
            foreach (var nonBlockMesh in nonBlockCollisionMeshes.GetMeshesForWorldSegment(
                         new GridPosition(i, j, k), entityId))
            {
                if (CheckSingleCollision(kinematicsIndex, entityMesh, posVel, ref supportedBelow, nonBlockMesh))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks for a single collision between a pair of meshes.
    /// </summary>
    /// <param name="kinematicsIndex"></param>
    /// <param name="entityMesh"></param>
    /// <param name="posVel"></param>
    /// <param name="supportedBelow"></param>
    /// <param name="otherMesh"></param>
    /// <returns></returns>
    private bool CheckSingleCollision(int kinematicsIndex, BoundingBox entityMesh, Kinematics posVel,
        ref bool supportedBelow, BoundingBox otherMesh)
    {
        if (!entityMesh.Intersects(otherMesh, out var resolvingTranslation, out var minAbsOverlap))
            return false;

        // This is an intersection or surface contact. They can be distinguished by the components
        // of the resolving translation: a surface contact will have one or more zero-valued components.
        var isSurfaceContact = minAbsOverlap < ContactTestEpsilon;
        if (!isSurfaceContact)
        {
            // Collision! Stop movement and resolve to the nearest surface contact.
            kinematics.Components[(ulong)kinematicsIndex] = new Kinematics
            {
                Position = posVel.Position + resolvingTranslation,
                Velocity = AdjustVelocityForCollision(resolvingTranslation, posVel.Velocity)
            };
            return true;
        }

        // If the bottom of the entity has the same z as the top of the block mesh...
        if (Math.Abs(entityMesh.Position.Z - (otherMesh.Position.Z + otherMesh.Size.Z)) < ContactTestEpsilon)
            // This mesh supports the entity from below, so inhibit gravity processing later.
            supportedBelow = true;
        return false;
    }

    /// <summary>
    ///     Adjusts entity velocity in response to a collision.
    /// </summary>
    /// <param name="resolvingTranslation">Entity translation that resolves the collision.</param>
    /// <param name="velocity">Current entity velocity.</param>
    /// <returns>Adjusted entity velocity.</returns>
    private Vector3 AdjustVelocityForCollision(Vector3 resolvingTranslation, Vector3 velocity)
    {
        return new Vector3
        {
            X = Math.Abs(resolvingTranslation.X) > 0.0f ? 0.0f : velocity.X,
            Y = Math.Abs(resolvingTranslation.Y) > 0.0f ? 0.0f : velocity.Y,
            Z = Math.Abs(resolvingTranslation.Z) > 0.0f ? 0.0f : velocity.Z
        };
    }

    /// <summary>
    ///     Applies gravitational acceleration up to terminal velocity.
    /// </summary>
    /// <param name="kinematicsIndex">Kinematics component index for entity.</param>
    private void ApplyGravity(int kinematicsIndex)
    {
        var posVel = kinematics.Components[(ulong)kinematicsIndex];
        var newVel = posVel.Velocity with
        {
            Z = Math.Max(posVel.Velocity.Z + GravityVelocityStep, PhysicsConstants.TerminalVelocity)
        };
        kinematics.Components[(ulong)kinematicsIndex] = posVel with { Velocity = newVel };
    }
}