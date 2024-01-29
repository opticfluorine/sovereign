// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Player.Components.Indexers;
using Sovereign.EngineCore.World;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Manages player subscriptions to nearby world segments.
/// </summary>
/// <remarks>
///     The subscription manager works by tapping into the component-level events
///     associated with player position. Any change in player position (including
///     player load/unload) results first in unsubscription from any world segments
///     which have gone out of range, followed by subscription to any world segments
///     which have come into range.
/// </remarks>
public class WorldSegmentSubscriptionManager
{
    private readonly WorldSegmentActivationManager activationManager;

    /// <summary>
    ///     Working dictionary for tracking the change in subscription count for each world segment.
    /// </summary>
    private readonly Dictionary<GridPosition, int> changeCounts = new();

    /// <summary>
    ///     Dictionary mapping player entity IDs to their current world segments.
    /// </summary>
    private readonly Dictionary<ulong, GridPosition> currentWorldSegments = new();

    private readonly IEventSender eventSender;
    private readonly EntityHierarchyIndexer hierarchyIndexer;
    private readonly WorldManagementInternalController internalController;

    /// <summary>
    ///     Map from world segment index to the set of players subscribed to that world segment.
    /// </summary>
    private readonly Dictionary<GridPosition, HashSet<ulong>> playersByWorldSegments = new();

    private readonly PlayerPositionEventFilter positionEventFilter;
    private readonly PositionComponentCollection positions;

    private readonly WorldSegmentResolver resolver;

    /// <summary>
    ///     Dictionary mapping player entity IDs to subscribed world segments.
    /// </summary>
    private readonly Dictionary<ulong, HashSet<GridPosition>> subscriptions = new();

    private readonly EntitySynchronizer synchronizer;

    private readonly WorldSegmentSynchronizationManager syncManager;

    private readonly IWorldManagementConfiguration worldConfig;

    public WorldSegmentSubscriptionManager(PlayerPositionEventFilter positionEventFilter,
        WorldSegmentResolver resolver, IWorldManagementConfiguration worldConfig,
        WorldSegmentActivationManager activationManager,
        IEventSender eventSender, WorldManagementInternalController internalController,
        WorldSegmentSynchronizationManager syncManager, PositionComponentCollection positions,
        EntitySynchronizer synchronizer, EntityHierarchyIndexer hierarchyIndexer,
        EntityTable entityTable)
    {
        this.positionEventFilter = positionEventFilter;
        this.resolver = resolver;
        this.worldConfig = worldConfig;
        this.activationManager = activationManager;
        this.eventSender = eventSender;
        this.internalController = internalController;
        this.syncManager = syncManager;
        this.positions = positions;
        this.synchronizer = synchronizer;
        this.hierarchyIndexer = hierarchyIndexer;

        // Register event handlers.
        this.positionEventFilter.OnStartUpdates += OnStartUpdates;
        this.positionEventFilter.OnEndUpdates += OnEndUpdates;
        this.positionEventFilter.OnComponentAdded += OnPlayerAdded;
        this.positionEventFilter.OnComponentModified += OnPlayerMoved;
        this.positionEventFilter.OnComponentRemoved += OnPlayerRemoved;
        entityTable.OnNonBlockEntityAdded += OnNonBlockEntityAdded;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Gets the players who are currently subscribed to the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Set of players (possibly empty) subscribed to the world segment.</returns>
    public IReadOnlySet<ulong> GetSubscribersForWorldSegment(GridPosition segmentIndex)
    {
        return playersByWorldSegments.TryGetValue(segmentIndex, out var players)
            ? players
            : new HashSet<ulong>();
    }

    /// <summary>
    ///     Called when a player is unloaded.
    /// </summary>
    /// <param name="entityId">Player entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void OnPlayerRemoved(ulong entityId, bool isUnload)
    {
        // Special case: for removal, unsubscribe from all.
        foreach (var segment in subscriptions[entityId])
        {
            if (changeCounts.ContainsKey(segment))
                changeCounts[segment] -= 1;
            else
                changeCounts[segment] = -1;
            playersByWorldSegments[segment].Remove(entityId);
        }

        // Clean up any info for the entity.
        subscriptions.Remove(entityId);
        currentWorldSegments.Remove(entityId);
    }

    /// <summary>
    ///     Called when a player position is updated.
    /// </summary>
    /// <param name="entityId">Player entity ID.</param>
    /// <param name="position">New position.</param>
    private void OnPlayerMoved(ulong entityId, Vector3 position)
    {
        // Check if the player moved to a different world segment.
        var center = resolver.GetWorldSegmentForPosition(position);
        if (currentWorldSegments[entityId] != center)
        {
            // Moved.
            currentWorldSegments[entityId] = center;
            DoUpdateLogic(entityId, center);
        }
    }

    /// <summary>
    ///     Called when a player entity is created/loaded.
    /// </summary>
    /// <param name="entityId">Player entity ID.</param>
    /// <param name="position">New position.</param>
    /// <param name="isLoad">Unused.</param>
    private void OnPlayerAdded(ulong entityId, Vector3 position, bool isLoad)
    {
        subscriptions[entityId] = new HashSet<GridPosition>();
        var center = resolver.GetWorldSegmentForPosition(position);
        currentWorldSegments[entityId] = center;
        DoUpdateLogic(entityId, center);
    }

    /// <summary>
    ///     Called when a batch of player position updates completes.
    /// </summary>
    private void OnEndUpdates()
    {
        activationManager.ProcessChanges(changeCounts);
    }

    /// <summary>
    ///     Called when a batch of player position updates begins.
    /// </summary>
    private void OnStartUpdates()
    {
        // Reset working structures to a clear state.
        changeCounts.Clear();
    }

    /// <summary>
    ///     Builds the set of world segments that are subscribable from a given center world segment.
    /// </summary>
    /// <param name="center">Center world segment.</param>
    private HashSet<GridPosition> BuildSubscriptionSet(GridPosition center)
    {
        var result = new HashSet<GridPosition>(worldConfig.SubscriptionRange * worldConfig.SubscriptionRange);
        for (var x = center.X - worldConfig.SubscriptionRange; x < center.X + worldConfig.SubscriptionRange + 1; ++x)
        for (var y = center.Y - worldConfig.SubscriptionRange;
             y < center.Y + worldConfig.SubscriptionRange + 1;
             ++y)
        for (var z = center.Z - worldConfig.SubscriptionRange;
             z < center.Z + worldConfig.SubscriptionRange + 1;
             ++z)
            result.Add(new GridPosition(x, y, z));

        return result;
    }

    /// <summary>
    ///     Processes a subscription update for a single player entity at its new world segment index.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="center">New world segment index for player entity.</param>
    private void DoUpdateLogic(ulong playerEntityId, GridPosition center)
    {
        var currentSubscriptionSet = subscriptions[playerEntityId];
        var newSubscriptionSet = BuildSubscriptionSet(center);

        // There are no changes for the intersection of the current and new subscription sets.
        var lastSubscriptionSet = new HashSet<GridPosition>(currentSubscriptionSet);
        var unchangedSet = new HashSet<GridPosition>(newSubscriptionSet);
        unchangedSet.IntersectWith(currentSubscriptionSet);

        // Subscribe to anything new that isn't in the intersection.
        foreach (var segment in newSubscriptionSet)
            if (!unchangedSet.Contains(segment))
            {
                Logger.DebugFormat("Subscribe {0} to {1}.", playerEntityId, segment);
                internalController.PushSubscribe(eventSender, playerEntityId, segment);
                currentSubscriptionSet.Add(segment);
                if (!playersByWorldSegments.ContainsKey(segment))
                    playersByWorldSegments[segment] = new HashSet<ulong>();
                playersByWorldSegments[segment].Add(playerEntityId);
                syncManager.OnPlayerSubscribe(playerEntityId, segment);
                if (changeCounts.ContainsKey(segment))
                    changeCounts[segment] += 1;
                else
                    changeCounts[segment] = 1;
            }

        // Unsubscribe from anything old that isn't in the intersection if the world segment is more than
        // one segment outside of the subscription radius. Adding this one-segment-wide gap between subscribe
        // and unsubscribe helps to prevent players from rapidly toggling the subscription through small
        // movements over the border.
        foreach (var segment in lastSubscriptionSet)
            if (!unchangedSet.Contains(segment))
            {
                // Beyond the subscribe radius, now check whether beyond the unsubscribe radius.
                var unsubRadius = worldConfig.SubscriptionRange + 1;
                var dx = Math.Abs(segment.X - center.X);
                var dy = Math.Abs(segment.Y - center.Y);
                var dz = Math.Abs(segment.Z - center.Z);
                if (dx > unsubRadius || dy > unsubRadius || dz > unsubRadius)
                {
                    // Beyond the unsubscribe radius, trigger unsubscribe.
                    Logger.DebugFormat("Unsubscribe {0} from {1}.", playerEntityId, segment);
                    internalController.PushUnsubscribe(eventSender, playerEntityId, segment);
                    currentSubscriptionSet.Remove(segment);
                    playersByWorldSegments[segment].Remove(playerEntityId);
                    if (changeCounts.ContainsKey(segment))
                        changeCounts[segment] -= 1;
                    else
                        changeCounts[segment] = -1;
                }
            }
    }

    /// <summary>
    ///     Called when an entity has been fully added to synchronize the entity with any subscribers.
    /// </summary>
    /// <param name="entityId">Entity.</param>
    private void OnNonBlockEntityAdded(ulong entityId)
    {
        // Skip if not a positioned entity.
        if (!positions.HasComponentForEntity(entityId)) return;

        // Skip if there are no subscribers to the relevant world segment.
        var segmentIndex = resolver.GetWorldSegmentForPosition(positions[entityId]);
        var playerEntityIds = GetSubscribersForWorldSegment(segmentIndex);
        if (playerEntityIds.Count == 0) return;

        // Identify the new entity tree.
        var entitiesToSync = hierarchyIndexer.GetAllDescendants(entityId);
        entitiesToSync.Add(entityId);
        synchronizer.Synchronize(playerEntityIds, entitiesToSync);
    }
}