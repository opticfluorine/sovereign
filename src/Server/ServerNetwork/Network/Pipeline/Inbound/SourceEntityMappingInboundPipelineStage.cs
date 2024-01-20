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
using System.Collections.Generic;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;

namespace Sovereign.ServerNetwork.Network.Pipeline.Inbound;

/// <summary>
///     Inbound network pipeline stage that maps an event to the player associated with the
///     connection from which the event was received.
/// </summary>
public class SourceEntityMappingInboundPipelineStage : IInboundPipelineStage
{
    private readonly AccountServices accountServices;

    /// <summary>
    ///     Entity mappers by event ID.
    /// </summary>
    private readonly Dictionary<EventId, Action<IEventDetails, ulong>> mappers;

    public SourceEntityMappingInboundPipelineStage(AccountServices accountServices)
    {
        this.accountServices = accountServices;

        mappers = new Dictionary<EventId, Action<IEventDetails, ulong>>
        {
            { EventId.Core_Movement_RequestMove, RequestMoveEventMapper }
        };
    }

    public int Priority => 100;
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (mappers.TryGetValue(ev.EventId, out var mapper))
        {
            // Mapper found, only process the event if it can be mapped back to a player.
            var playerEntityId = accountServices.GetPlayerForConnectionId(connection.Id);
            if (!playerEntityId.HasValue || ev.EventDetails == null) return;
            mapper(ev.EventDetails, playerEntityId.Value);
        }

        NextStage?.ProcessEvent(ev, connection);
    }

    /// <summary>
    ///     Mapper for RequestMoveEventDetails events.
    /// </summary>
    /// <param name="details">Event details.</param>
    /// <param name="entityId">Player entity ID.</param>
    private static void RequestMoveEventMapper(IEventDetails details, ulong entityId)
    {
        ((RequestMoveEventDetails)details).EntityId = entityId;
    }
}