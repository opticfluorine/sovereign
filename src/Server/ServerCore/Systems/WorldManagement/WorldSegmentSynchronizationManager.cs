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
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Player.Components;
using Sovereign.EngineCore.Systems.WorldManagement.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for synchronizing world segments with clients.
/// </summary>
public class WorldSegmentSynchronizationManager
{
    private readonly WorldSegmentActivationManager activationManager;
    private readonly EntityHierarchyIndexer hierarchyIndexer;

    /// <summary>
    ///     Map from segment index to a queue of player entity IDs needing synchronization.
    /// </summary>
    private readonly Dictionary<GridPosition, Queue<ulong>> pendingPlayersBySegment = new();

    private readonly PlayerCharacterTagCollection playerCharacters;

    private readonly EntitySynchronizer synchronizer;
    private readonly NonBlockWorldSegmentIndexer worldSegmentIndexer;

    public WorldSegmentSynchronizationManager(WorldSegmentActivationManager activationManager,
        EntitySynchronizer synchronizer, NonBlockWorldSegmentIndexer worldSegmentIndexer,
        EntityHierarchyIndexer hierarchyIndexer, PlayerCharacterTagCollection playerCharacters)
    {
        this.activationManager = activationManager;
        this.synchronizer = synchronizer;
        this.worldSegmentIndexer = worldSegmentIndexer;
        this.hierarchyIndexer = hierarchyIndexer;
        this.playerCharacters = playerCharacters;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Called when a player has subscribed to a world segment to initiate the
    ///     non-block entity synchronization for that segment.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void OnPlayerSubscribe(ulong playerEntityId, GridPosition segmentIndex)
    {
        if (activationManager.IsWorldSegmentLoaded(segmentIndex))
        {
            // World segment is already loaded, so we can immediately send the synchronization events.
            Logger.DebugFormat("Segment already loaded; sync {0} to {1}.", segmentIndex, playerEntityId);
            SendSynchronizationEvents(playerEntityId, segmentIndex);
        }
        else
        {
            // World segment still loading, so enqueue for later synchronization.
            Logger.DebugFormat("Segment load in process; enqueue {0} for {1}.", segmentIndex, playerEntityId);
            if (!pendingPlayersBySegment.ContainsKey(segmentIndex))
                pendingPlayersBySegment[segmentIndex] = new Queue<ulong>();

            pendingPlayersBySegment[segmentIndex].Enqueue(playerEntityId);
        }
    }

    /// <summary>
    ///     Called when a world segment has been loaded into memory.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void OnWorldSegmentLoaded(GridPosition segmentIndex)
    {
        // Process synchronization for any players that were waiting on the loaded world segment.
        if (pendingPlayersBySegment.TryGetValue(segmentIndex, out var queue))
        {
            Logger.DebugFormat("Processing pending syncs for newly loaded segment {0}.", segmentIndex);
            while (queue.TryDequeue(out var playerEntityId)) SendSynchronizationEvents(playerEntityId, segmentIndex);
        }
    }

    /// <summary>
    ///     Asynchronously ends non-block entity synchronization events for the given world segment to the given player.
    /// </summary>
    /// <param name="playerEntityId">Player to synchronize.</param>
    /// <param name="segmentIndex">World segment index to synchronize.</param>
    private void SendSynchronizationEvents(ulong playerEntityId, GridPosition segmentIndex)
    {
        // Fill in the hierarchy beneath the entities.
        var allEntities = worldSegmentIndexer.GetEntitiesInWorldSegment(segmentIndex)
            .SelectMany(entityId =>
            {
                var all = hierarchyIndexer.GetAllDescendants(entityId);
                all.Add(entityId);
                return all;
            });

        // Synchronize all entities.
        synchronizer.Synchronize(playerEntityId, allEntities);
    }

    /// <summary>
    ///     Called once Persistence has fully loaded a player entity tree on login.
    /// </summary>
    /// <param name="entityId">Player entity ID.</param>
    public void OnPlayerLoaded(ulong entityId)
    {
        // Synchronize the newly loaded player entity with the player in case the load completed
        // after the initial subscription-driven synchronizations.
        if (!playerCharacters.HasTagForEntity(entityId)) return;
        var entities = hierarchyIndexer.GetAllDescendants(entityId);
        entities.Add(entityId);
        synchronizer.Synchronize(entityId, entities);
    }
}