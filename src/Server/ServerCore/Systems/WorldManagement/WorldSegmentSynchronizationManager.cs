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
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for synchronizing world segments with clients.
/// </summary>
public class WorldSegmentSynchronizationManager
{
    private readonly WorldSegmentActivationManager activationManager;

    /// <summary>
    ///     Map from segment index to a queue of player entity IDs needing synchronization.
    /// </summary>
    private readonly Dictionary<GridPosition, Queue<ulong>> pendingPlayersBySegment = new();

    public WorldSegmentSynchronizationManager(WorldSegmentActivationManager activationManager)
    {
        this.activationManager = activationManager;
    }

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
            SendSynchronizationEvents(playerEntityId, segmentIndex);
        }
        else
        {
            // World segment still loading, so enqueue for later synchronization.
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
            while (queue.TryDequeue(out var playerEntityId))
            {
                SendSynchronizationEvents(playerEntityId, segmentIndex);
            }
        }
    }

    /// <summary>
    ///     Asynchronously ends non-block entity synchronization events for the given world segment to the given player.
    /// </summary>
    /// <param name="playerEntityId">Player to synchronize.</param>
    /// <param name="segmentIndex">World segment index to synchronize.</param>
    private void SendSynchronizationEvents(ulong playerEntityId, GridPosition segmentIndex)
    {
    }
}