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
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Connection mapper that broadcasts an event to every entity in a world segment.
/// </summary>
public class WorldSegmentConnectionMapper : ISpecificConnectionMapper
{
    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly Func<OutboundEventInfo, Maybe<GridPosition>> mapper;
    private readonly uint radius;
    private readonly RegionalConnectionMapCache regionalConnectionMapCache;

    /// <summary>
    ///     Creates a connection mapper.
    /// </summary>
    /// <param name="accountServices">Account services.</param>
    /// <param name="connectionManager">Connection manager.</param>
    /// <param name="radius">Radius around world segment to include.</param>
    /// <param name="regionalConnectionMapCache">Regional mapping cache.</param>
    /// <param name="mapper">Function taking an outbound event to the associated world segment.</param>
    public WorldSegmentConnectionMapper(
        AccountServices accountServices, NetworkConnectionManager connectionManager,
        RegionalConnectionMapCache regionalConnectionMapCache, uint radius,
        Func<OutboundEventInfo, Maybe<GridPosition>> mapper)
    {
        this.accountServices = accountServices;
        this.connectionManager = connectionManager;
        this.regionalConnectionMapCache = regionalConnectionMapCache;
        this.radius = radius;
        this.mapper = mapper;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Send the event to each player subscribed to the same world segment.
        var segmentIndex = mapper.Invoke(evInfo);
        if (!segmentIndex.HasValue) return;

        var subscribedPlayers = regionalConnectionMapCache.GetPlayersNearWorldSegment(segmentIndex.Value, radius);
        foreach (var playerEntityId in subscribedPlayers)
        {
            var connectionId = accountServices.GetConnectionIdForPlayer(playerEntityId);
            if (connectionId.HasValue)
            {
                var connection = connectionManager.GetConnection(connectionId.Value);
                NextStage?.Process(new OutboundEventInfo(evInfo, connection));
            }
        }
    }

    public IOutboundPipelineStage? NextStage { get; set; }
}