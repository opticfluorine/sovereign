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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.ServerNetwork.Network.ServerNetwork;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerNetwork.Network.Pipeline.Inbound;

/// <summary>
///     Enforces a strict allowlist of permitted event types for events received
///     from a client by the event server.
/// </summary>
public class ServerAllowedEventsInboundPipelineStage : IInboundPipelineStage
{
    /// <summary>
    ///     Set of allowed event IDs for received network events.
    /// </summary>
    private readonly HashSet<EventId> allowList = new()
    {
        EventId.Core_Ping_Pong,
        EventId.Core_Movement_RequestMove,
        EventId.Core_Network_Logout,
        EventId.Core_Chat_Send,
        EventId.Server_TemplateEntity_Update,
        EventId.Server_WorldEdit_SetBlock,
        EventId.Server_WorldEdit_RemoveBlock
    };

    private readonly IEventSender eventSender;
    private readonly ILogger<ServerAllowedEventsInboundPipelineStage> logger;

    private readonly ServerNetworkController networkController;

    public ServerAllowedEventsInboundPipelineStage(IEventSender eventSender, ServerNetworkController networkController,
        ILogger<ServerAllowedEventsInboundPipelineStage> logger)
    {
        this.eventSender = eventSender;
        this.networkController = networkController;
        this.logger = logger;
    }

    public int Priority => int.MinValue; // always first stage
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (allowList.Contains(ev.EventId))
            // Event type is permitted, pass to the next stage of the pipeline.
        {
            NextStage?.ProcessEvent(ev, connection);
        }
        else
        {
            // Event type is not permitted, log warning and drop the event.
            logger.LogWarning("Rejecting network event with ID {Id} not found on allowlist.", ev.EventId);

            // This is a breach of protocol and could be a security issue, so forcibly disconnect the client.
            networkController.Disconnect(eventSender, ev.FromConnectionId);
        }
    }
}