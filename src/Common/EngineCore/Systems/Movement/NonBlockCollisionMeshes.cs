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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Manages the collision meshes for non-block entities.
/// </summary>
public sealed class NonBlockCollisionMeshes
{
    private readonly BoundingBoxComponentCollection boundingBoxes;

    private readonly IEnumerable<BoundingBox> empty = [];
    private readonly Dictionary<ulong, GridPosition> knownWorldSegments = new();
    private readonly Dictionary<GridPosition, Dictionary<ulong, BoundingBox>> meshesByWorldSegment = new();
    private readonly WorldSegmentResolver resolver;

    public NonBlockCollisionMeshes(KinematicsComponentCollection kinematics,
        BoundingBoxComponentCollection boundingBoxes, WorldSegmentResolver resolver)
    {
        this.boundingBoxes = boundingBoxes;
        this.resolver = resolver;

        kinematics.OnComponentAdded += OnAdd;
        kinematics.OnComponentModified += OnChange;
        kinematics.OnComponentRemoved += OnRemove;
    }

    /// <summary>
    ///     Gets the collision meshes for non-block entities in the given world segment
    ///     while excluding self-collisions.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="selfEntityId">Self entity ID.</param>
    /// <returns></returns>
    public IEnumerable<BoundingBox> GetMeshesForWorldSegment(GridPosition segmentIndex, ulong selfEntityId)
    {
        if (!meshesByWorldSegment.TryGetValue(segmentIndex, out var segment)) return empty;

        var selfPresent = knownWorldSegments.TryGetValue(selfEntityId, out var selfSegment) &&
                          segmentIndex == selfSegment;
        return selfPresent ? FilteredMeshes(segment, selfEntityId) : segment.Values;
    }

    private IEnumerable<BoundingBox> FilteredMeshes(Dictionary<ulong, BoundingBox> segment, ulong selfEntityId)
    {
        foreach (var kvp in segment)
        {
            if (kvp.Key == selfEntityId) continue;
            yield return kvp.Value;
        }
    }

    /// <summary>
    ///     Called when a Kinematics component is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">New Kinematics value.</param>
    /// <param name="isLoad">Unused.</param>
    private void OnAdd(ulong entityId, Kinematics componentValue, bool isLoad)
    {
        OnChange(entityId, componentValue);
    }

    /// <summary>
    ///     Called when a Kinematics component is modified.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">New Kinematics value.</param>
    private void OnChange(ulong entityId, Kinematics componentValue)
    {
        if (entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId) return;

        // Note that for performance reasons, we don't handle the case where an entity previously had a bounding
        // box but no longer does. This will leave a static collision mesh in the entity's last location at the
        // time the bounding box was removed. Do not remove bounding boxes from entities!
        if (!boundingBoxes.TryGetValue(entityId, out var boundingBox)) return;

        // If entity moved to a new world segment, move its mesh.
        var segmentIndex = resolver.GetWorldSegmentForPosition(componentValue.Position);
        if (knownWorldSegments.TryGetValue(entityId, out var oldSegmentIndex) &&
            segmentIndex != oldSegmentIndex &&
            meshesByWorldSegment.TryGetValue(oldSegmentIndex, out var oldSegment))
            oldSegment.Remove(entityId);

        // Grab a new dict if this is the first non-block entity in its world segment.
        if (!meshesByWorldSegment.TryGetValue(segmentIndex, out var segment))
        {
            segment = new Dictionary<ulong, BoundingBox>();
            meshesByWorldSegment[segmentIndex] = segment;
        }

        // Update mesh by translating the bounding box to the entity's new position.
        segment[entityId] = boundingBox.Translate(componentValue.Position);
        knownWorldSegments[entityId] = segmentIndex;
    }

    /// <summary>
    ///     Called when a Kinematics component is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isLoad">Unused.</param>
    private void OnRemove(ulong entityId, bool isLoad)
    {
        if (knownWorldSegments.Remove(entityId, out var segmentIndex) &&
            meshesByWorldSegment.TryGetValue(segmentIndex, out var segment))
            segment.Remove(entityId);
    }
}