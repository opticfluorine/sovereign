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
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Player;
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
    private readonly EntityTable entityTable;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ILogger logger;
    private readonly ParentComponentCollection parents;
    private readonly WorldSegmentResolver resolver;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ServerOnlyTagCollection serverOnly;
    private readonly WorldManagementServices worldManagementServices;

    /// <summary>
    ///     Creates a specific world segment connection mapper.
    /// </summary>
    /// <param name="entitySelector">Function that gets the associated entity for the event.</param>
    internal EntityWorldSegmentConnectionMapper(
        KinematicsComponentCollection kinematics,
        ParentComponentCollection parents,
        EntityTable entityTable,
        WorldSegmentResolver resolver,
        WorldManagementServices worldManagementServices,
        NetworkConnectionManager connectionManager,
        AccountServices accountServices,
        ServerOnlyTagCollection serverOnly,
        PlayerRoleCheck roleCheck,
        ILogger logger,
        Func<OutboundEventInfo, Maybe<ulong>> entitySelector)
    {
        this.kinematics = kinematics;
        this.parents = parents;
        this.entityTable = entityTable;
        this.resolver = resolver;
        this.worldManagementServices = worldManagementServices;
        this.connectionManager = connectionManager;
        this.accountServices = accountServices;
        this.serverOnly = serverOnly;
        this.roleCheck = roleCheck;
        this.logger = logger;
        this.entitySelector = entitySelector;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Locate the world segment of the source entity.
        var entityId = entitySelector.Invoke(evInfo);
        if (!entityId.HasValue)
        {
            logger.LogError("Could not retrieve entity ID from event details for event type {Id}.",
                evInfo.Event.EventId);
            return;
        }

        // Skip if the source entity no longer exists.
        if (!entityTable.Exists(entityId.Value)) return;

        if (!kinematics.TryFindNearest(entityId.Value, parents, out var posVel, out _))
        {
            logger.LogError("Can't broadcast to world segment for non-positioned entity {Id:X}.", entityId.Value);
            return;
        }

        var isServerOnly = serverOnly.HasTagForEntity(entityId.Value);
        var segmentIndex = resolver.GetWorldSegmentForPosition(posVel.Position);

        // Send the event to each player subscribed to the same world segment.
        var subscribedPlayers = worldManagementServices.GetPlayersSubscribedToWorldSegment(segmentIndex);
        foreach (var playerEntityId in subscribedPlayers)
        {
            // Avoid sending updates for server-only entities to subscribers who can't see them.
            if (isServerOnly && !roleCheck.IsPlayerAdmin(playerEntityId)) continue;

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