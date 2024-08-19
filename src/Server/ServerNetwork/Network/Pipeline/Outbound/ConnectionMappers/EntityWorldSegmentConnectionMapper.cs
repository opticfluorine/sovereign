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
using Castle.Core.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Generic connection mapper that sends an event to any players who are
///     subscribed to the world segment where a non-block entity is located. This
///     mapper type is preferred when broadcasting events from a non-block
///     entity.
/// </summary>
public class EntityWorldSegmentConnectionMapper : ISpecificConnectionMapper
{
    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly Func<OutboundEventInfo, Maybe<ulong>> entitySelector;
    private readonly KinematicComponentCollection kinematics;
    private readonly ILogger logger;
    private readonly WorldSegmentResolver resolver;
    private readonly WorldManagementServices worldManagementServices;

    /// <summary>
    ///     Creates a specific world segment connection mapper.
    /// </summary>
    /// <param name="entitySelector">Function that gets the associated entity for the event.</param>
    internal EntityWorldSegmentConnectionMapper(
        KinematicComponentCollection kinematics,
        WorldSegmentResolver resolver,
        WorldManagementServices worldManagementServices,
        NetworkConnectionManager connectionManager,
        AccountServices accountServices,
        ILogger logger,
        Func<OutboundEventInfo, Maybe<ulong>> entitySelector)
    {
        this.kinematics = kinematics;
        this.resolver = resolver;
        this.worldManagementServices = worldManagementServices;
        this.connectionManager = connectionManager;
        this.accountServices = accountServices;
        this.logger = logger;
        this.entitySelector = entitySelector;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Locate the world segment of the source entity.
        var entityId = entitySelector.Invoke(evInfo);
        if (!entityId.HasValue)
        {
            logger.ErrorFormat("Could not retrieve entity ID from event details for event type {0}.",
                evInfo.Event.EventId);
            return;
        }

        if (!kinematics.HasComponentForEntity(entityId.Value))
        {
            logger.ErrorFormat("Can't broadcast to world segment for non-positioned entity {0}.", entityId);
            return;
        }

        var segmentIndex = resolver.GetWorldSegmentForPosition(kinematics[entityId.Value].Position);

        // Send the event to each player subscribed to the same world segment.
        var subscribedPlayers = worldManagementServices.GetPlayersSubscribedToWorldSegment(segmentIndex);
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