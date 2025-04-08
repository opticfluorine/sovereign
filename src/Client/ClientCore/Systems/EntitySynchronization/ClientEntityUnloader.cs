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
using System.Linq;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.WorldManagement;

namespace Sovereign.ClientCore.Systems.EntitySynchronization;

/// <summary>
///     Responsible for unloading entities from memory for the client.
/// </summary>
public class ClientEntityUnloader
{
    private readonly BlockWorldSegmentIndexer blockSegmentIndexer;
    private readonly EntityManager entityManager;
    private readonly IEventSender eventSender;
    private readonly EntityHierarchyIndexer hierarchyIndexer;
    private readonly NonBlockWorldSegmentIndexer nonBlockSegmentIndexer;

    /// <summary>
    ///     Set of currently subscribed world segments.
    /// </summary>
    private readonly HashSet<GridPosition> subscribedSegments = new();

    private readonly CoreWorldManagementController worldController;

    /// <summary>
    ///     Current player entity ID.
    /// </summary>
    private ulong playerEntityId;

    public ClientEntityUnloader(BlockWorldSegmentIndexer blockSegmentIndexer,
        NonBlockWorldSegmentIndexer nonBlockSegmentIndexer,
        EntityHierarchyIndexer hierarchyIndexer, EntityManager entityManager,
        CoreWorldManagementController worldController, IEventSender eventSender)
    {
        this.blockSegmentIndexer = blockSegmentIndexer;
        this.nonBlockSegmentIndexer = nonBlockSegmentIndexer;
        this.hierarchyIndexer = hierarchyIndexer;
        this.entityManager = entityManager;
        this.worldController = worldController;
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Sets the current player entity ID to prevent accidental unloading in high-latency situations.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void SetPlayer(ulong playerEntityId)
    {
        this.playerEntityId = playerEntityId;
    }

    /// <summary>
    ///     Called when an entity leaves a world segment.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="newSegmentIndex">Segment index of the world segment entered by the entity.</param>
    public void OnEntityChangeWorldSegment(ulong entityId, GridPosition newSegmentIndex)
    {
        // If the entity made an authoritative move to a segment we aren't subscribed to, unload it.
        if (!subscribedSegments.Contains(newSegmentIndex)) UnloadEntity(entityId);
    }

    /// <summary>
    ///     Called when the player is subscribed to a world segment.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    public void OnSubscribe(GridPosition segmentIndex)
    {
        subscribedSegments.Add(segmentIndex);
    }

    /// <summary>
    ///     Called when the player is unsubscribed from a world segment.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    public void OnUnsubscribe(GridPosition segmentIndex)
    {
        subscribedSegments.Remove(segmentIndex);
        UnloadWorldSegment(segmentIndex);
    }

    /// <summary>
    ///     Called when the server directs the client to desynchronize an entity tree.
    /// </summary>
    /// <param name="entityId">Root of the entity tree to be desynchronized.</param>
    public void OnDesync(ulong entityId)
    {
        UnloadEntity(entityId);
    }

    /// <summary>
    ///     Called when the client is informed that an entity has teleported.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="toSegmentIndex">World segment index of teleportation target.</param>
    public void OnEntityTeleported(ulong entityId, GridPosition toSegmentIndex)
    {
        if (subscribedSegments.Contains(toSegmentIndex)) return;
        
        // Entity has teleported out of the subscribed area, unload from the client.
        UnloadEntity(entityId);
    }
    
    /// <summary>
    ///     Unsubscribes from all world segments.
    /// </summary>
    public void UnsubscribeAll()
    {
        foreach (var segmentIndex in subscribedSegments) UnloadWorldSegment(segmentIndex);
    }

    /// <summary>
    ///     Unloads all entities from the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void UnloadWorldSegment(GridPosition segmentIndex)
    {
        // Get all positioned entities in the segment, excluding the player (which can occur rarely if the
        // client is far behind the server when the unsubscribe event is received).
        // Then join them with any descendants.
        var entitiesToUnload = blockSegmentIndexer.GetEntitiesInWorldSegment(segmentIndex)
            .SelectMany(GetDescendantsAndSelf)
            .Concat(nonBlockSegmentIndexer.GetEntitiesInWorldSegment(segmentIndex)
                .Where(entityId => entityId != playerEntityId)
                .SelectMany(GetDescendantsAndSelf)
            );
        foreach (var entityId in entitiesToUnload) entityManager.UnloadEntity(entityId);

        worldController.AnnounceWorldSegmentUnloaded(eventSender, segmentIndex);
    }

    /// <summary>
    ///     Gets the full entity tree for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Full entity set.</returns>
    private HashSet<ulong> GetDescendantsAndSelf(ulong entityId)
    {
        var descendants = hierarchyIndexer.GetAllDescendants(entityId);
        descendants.Add(entityId);
        return descendants;
    }

    /// <summary>
    ///     Unloads a single entity and its descendants.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void UnloadEntity(ulong entityId)
    {
        var entitiesToUnload = hierarchyIndexer.GetAllDescendants(entityId);
        entitiesToUnload.Add(entityId);
        foreach (var nextEntityId in entitiesToUnload) entityManager.UnloadEntity(nextEntityId);
    }

}