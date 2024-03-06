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

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ClientCore.Systems.ClientNetwork;

/// <summary>
///     Manages client-side world segment subscriptions.
/// </summary>
public class ClientWorldSegmentSubscriptionManager
{
    private readonly WorldSegmentDataClient dataClient;
    private readonly HashSet<GridPosition> subscribedSegments = new();

    public ClientWorldSegmentSubscriptionManager(WorldSegmentDataClient dataClient)
    {
        this.dataClient = dataClient;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Subscribes the client to a world segment.
    /// </summary>
    /// <param name="worldSegmentIndex">World segment index.</param>
    public void Subscribe(GridPosition worldSegmentIndex)
    {
        if (!subscribedSegments.Contains(worldSegmentIndex))
        {
            Logger.DebugFormat("Subscribe to world segment {0}.", worldSegmentIndex);
            subscribedSegments.Add(worldSegmentIndex);
            dataClient.LoadSegment(worldSegmentIndex);
        }
        else
        {
            Logger.WarnFormat("Multiple subscribe to world segment {0}.", worldSegmentIndex);
        }
    }

    /// <summary>
    ///     Unsubscribes the client from a world segment.
    /// </summary>
    /// <param name="worldSegmentIndex">World segment index.</param>
    public void Unsubscribe(GridPosition worldSegmentIndex)
    {
        if (subscribedSegments.Contains(worldSegmentIndex))
        {
            Logger.DebugFormat("Unsubscribe from world segment {0}.", worldSegmentIndex);
            subscribedSegments.Remove(worldSegmentIndex);
        }
        else
        {
            Logger.WarnFormat("Unsubscribe from non-subscribed world segment {0}.", worldSegmentIndex);
        }
    }

    /// <summary>
    ///     Unsubscribes from all world segments.
    /// </summary>
    public void UnsubscribeAll()
    {
        foreach (var segmentIndex in subscribedSegments) Unsubscribe(segmentIndex);
    }
}