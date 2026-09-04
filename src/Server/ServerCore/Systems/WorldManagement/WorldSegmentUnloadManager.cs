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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.Timing;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for unloading the entities of idle world segments from server memory.
/// </summary>
/// <remarks>
///     A world segment is idle if it currently has zero subscribers and is not on the auto load
///     list. Unloading frees the segment's block and non-block entities, its cached summary block
///     data, and its activation state; the persisted data is not modified. A later subscription
///     to the segment reloads it from the database.
/// </remarks>
public class WorldSegmentUnloadManager
{
    private const ulong UsPerSecond = 1_000_000;

    private readonly WorldSegmentActivationManager activationManager;

    /// <summary>
    ///     Set of world segments configured to load automatically at startup.
    /// </summary>
    private readonly HashSet<GridPosition> autoLoadSegments;

    private readonly BlockWorldSegmentIndexer blockSegmentIndexer;
    private readonly WorldSegmentBlockDataManager blockDataManager;
    private readonly EntityManager entityManager;
    private readonly IEventSender eventSender;
    private readonly EntityHierarchyIndexer hierarchyIndexer;
    private readonly ILogger<WorldSegmentUnloadManager> logger;
    private readonly NonBlockWorldSegmentIndexer nonBlockSegmentIndexer;
    private readonly PlayerCharacterTagCollection playerTags;
    private readonly WorldSegmentRegistry registry;
    private readonly WorldSegmentSubscriptionManager subscriptionManager;
    private readonly ISystemTimer systemTimer;
    private readonly WorldManagementController worldController;
    private readonly WorldOptions worldOptions;

    public WorldSegmentUnloadManager(ISystemTimer systemTimer, IOptions<WorldOptions> worldOptions,
        IEventSender eventSender, WorldSegmentSubscriptionManager subscriptionManager,
        WorldSegmentActivationManager activationManager, WorldSegmentBlockDataManager blockDataManager,
        WorldSegmentRegistry registry, NonBlockWorldSegmentIndexer nonBlockSegmentIndexer,
        BlockWorldSegmentIndexer blockSegmentIndexer, EntityHierarchyIndexer hierarchyIndexer,
        EntityManager entityManager, PlayerCharacterTagCollection playerTags,
        WorldManagementController worldController, ILogger<WorldSegmentUnloadManager> logger)
    {
        this.systemTimer = systemTimer;
        this.worldOptions = worldOptions.Value;
        this.eventSender = eventSender;
        this.subscriptionManager = subscriptionManager;
        this.activationManager = activationManager;
        this.blockDataManager = blockDataManager;
        this.registry = registry;
        this.nonBlockSegmentIndexer = nonBlockSegmentIndexer;
        this.blockSegmentIndexer = blockSegmentIndexer;
        this.hierarchyIndexer = hierarchyIndexer;
        this.entityManager = entityManager;
        this.playerTags = playerTags;
        this.worldController = worldController;
        this.logger = logger;

        autoLoadSegments = new HashSet<GridPosition>(this.worldOptions.AutoLoadWorldSegments);
    }

    /// <summary>
    ///     Schedules the next periodic idle world segment unload check.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void ScheduleNextUnloadCheck(IEventSender eventSender)
    {
        var nextTime = systemTimer.GetTime()
                       + (ulong)worldOptions.EntityUnloadCheckIntervalSeconds * UsPerSecond;

        var ev = new Event(EventId.Server_WorldManagement_UnloadIdleWorldSegments,
            new BooleanEventDetails { Value = false }, nextTime);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Unloads the entities of all eligible idle world segments.
    /// </summary>
    /// <param name="ignoreCutoff">
    ///     If true, the zero-subscriber age cutoff is ignored so that all zero-subscriber,
    ///     non-auto-load segments are unloaded regardless of how long they have been idle.
    /// </param>
    public void UnloadIdleSegments(bool ignoreCutoff)
    {
        var now = systemTimer.GetTime();
        var cutoffUs = (ulong)worldOptions.EntityUnloadCutoffSeconds * UsPerSecond;

        // Snapshot the zero-subscriber tracking table to avoid mutating it during iteration.
        var candidateSegments = subscriptionManager.GetZeroSubscriberSegments();

        var unloadCount = 0;
        foreach (var segmentIndex in candidateSegments)
        {
            // Never unload auto load segments.
            if (autoLoadSegments.Contains(segmentIndex)) continue;

            // Skip segments that are not currently loaded; their load may still be in flight.
            // Any stale tracking entry will be cleaned up on a later pass.
            if (!activationManager.IsWorldSegmentLoaded(segmentIndex)) continue;

            // Skip segments with unsaved block data changes; unloading them now would lose
            // the changes when the block data is regenerated from the unloaded blocks.
            if (blockDataManager.HasPendingBlockDataUpdates(segmentIndex))
            {
                logger.LogDebug(
                    "Skipping unload of world segment {Index} with pending block data updates.",
                    segmentIndex);
                continue;
            }

            // Skip segments that have not yet reached the zero-subscriber age cutoff.
            if (!ignoreCutoff &&
                (!subscriptionManager.TryGetZeroSubscriberTime(segmentIndex, out var zeroTime) ||
                 now - zeroTime < cutoffUs))
                continue;

            UnloadSegment(segmentIndex);
            subscriptionManager.ClearZeroSubscriberSegment(segmentIndex);
            ++unloadCount;
        }

        if (unloadCount > 0)
            logger.LogInformation("Unloaded entities for {Count} idle world segments.", unloadCount);
        else
            logger.LogDebug("No idle world segments were eligible for unload.");
    }

    /// <summary>
    ///     Unloads all entities in the given world segment from server memory.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    private void UnloadSegment(GridPosition segmentIndex)
    {
        // Materialize the set of entities to unload before unloading anything so that the
        // world segment indexers are not mutated while they are being enumerated.
        var toUnload = new HashSet<ulong>();
        GatherEntitiesToUnload(blockSegmentIndexer.GetEntitiesInWorldSegment(segmentIndex), toUnload);
        GatherEntitiesToUnload(nonBlockSegmentIndexer.GetEntitiesInWorldSegment(segmentIndex), toUnload);

        foreach (var entityId in toUnload)
        {
            // Skip template entities defensively.
            if (entityId is >= EntityConstants.FirstTemplateEntityId
                and <= EntityConstants.LastTemplateEntityId) continue;

            entityManager.UnloadEntity(entityId);
        }

        // Free the cached summary block data for the segment.
        blockDataManager.RemoveWorldSegment(segmentIndex);

        // Deactivate the segment so that a later subscription reloads it from the database.
        activationManager.DeactivateWorldSegment(segmentIndex);

        // Keep the world segment registry consistent.
        registry.OnSegmentUnloaded(segmentIndex);

        // Announce the unload latched to the next tick so that the segment is only dropped
        // from the active segment sets after the unloads commit.
        worldController.AnnounceWorldSegmentUnloaded(eventSender, segmentIndex);

        logger.LogDebug("Unloaded world segment {Index}.", segmentIndex);
    }

    /// <summary>
    ///     Adds the entity trees rooted at the given positioned entities to the set of entities
    ///     to unload, excluding player characters.
    /// </summary>
    /// <param name="rootEntities">Positioned root entities in the world segment.</param>
    /// <param name="toUnload">Set to populate with entity IDs to unload.</param>
    private void GatherEntitiesToUnload(HashSet<ulong> rootEntities, HashSet<ulong> toUnload)
    {
        foreach (var rootId in rootEntities)
        {
            // Skip entire trees rooted at player characters; players are managed by the
            // account system rather than by world segment loads.
            if (playerTags.HasTagForEntity(rootId)) continue;

            toUnload.UnionWith(hierarchyIndexer.GetAllDescendants(rootId));
            toUnload.Add(rootId);
        }
    }
}