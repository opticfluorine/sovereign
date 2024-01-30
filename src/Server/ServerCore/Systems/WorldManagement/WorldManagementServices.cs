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
using System.Threading.Tasks;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Provides a public API for accessing world management data.
/// </summary>
public class WorldManagementServices
{
    private readonly WorldSegmentActivationManager activationManager;
    private readonly WorldSegmentBlockDataManager dataManager;
    private readonly WorldSegmentSubscriptionManager subscriptionManager;

    public WorldManagementServices(WorldSegmentSubscriptionManager subscriptionManager,
        WorldSegmentActivationManager activationManager,
        WorldSegmentBlockDataManager dataManager)
    {
        this.subscriptionManager = subscriptionManager;
        this.activationManager = activationManager;
        this.dataManager = dataManager;
    }

    /// <summary>
    ///     Gets the players currently subscribed to the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Set of players (possibly empty) subscribed to the world segment.</returns>
    public IReadOnlySet<ulong> GetPlayersSubscribedToWorldSegment(GridPosition segmentIndex)
    {
        return subscriptionManager.GetSubscribersForWorldSegment(segmentIndex);
    }

    /// <summary>
    ///     Checks whether the given world segment is fully loaded.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>true if fully loaded, false otherwise.</returns>
    public bool IsWorldSegmentLoaded(GridPosition segmentIndex)
    {
        return activationManager.IsWorldSegmentLoaded(segmentIndex);
    }

    /// <summary>
    ///     Gets the background task that yields the world segment block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Background task yielding block data.</returns>
    public Task<byte[]?>? GetWorldSegmentBlockData(GridPosition segmentIndex)
    {
        return dataManager.GetWorldSegmentBlockData(segmentIndex);
    }
}