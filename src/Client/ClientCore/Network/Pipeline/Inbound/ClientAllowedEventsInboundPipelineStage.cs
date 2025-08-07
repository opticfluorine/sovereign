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
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Network.Pipeline.Inbound;

/// <summary>
///     Enforces a strict allowlist of permitted event types for events received
///     from the event server by the client.
/// </summary>
public class ClientAllowedEventsInboundPipelineStage : IInboundPipelineStage
{
    /// <summary>
    ///     Set of allowed event IDs for received network events.
    /// </summary>
    private readonly HashSet<EventId> allowList = new()
    {
        EventId.Core_Ping_Ping,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Core_WorldManagement_Unsubscribe,
        EventId.Core_EntitySync_Sync,
        EventId.Core_EntitySync_Desync,
        EventId.Core_EntitySync_SyncTemplate,
        EventId.Core_Movement_Move,
        EventId.Core_Movement_TeleportNotice,
        EventId.Core_WorldManagement_EntityLeaveWorldSegment,
        EventId.Core_Chat_Global,
        EventId.Core_Chat_Local,
        EventId.Core_Chat_System,
        EventId.Core_Chat_Generic,
        EventId.Core_Block_ModifyNotice,
        EventId.Core_Block_RemoveNotice,
        EventId.Core_Time_Clock,
        EventId.Client_Dialogue_Enqueue
    };

    private readonly ILogger<ClientAllowedEventsInboundPipelineStage> logger;

    public ClientAllowedEventsInboundPipelineStage(ILogger<ClientAllowedEventsInboundPipelineStage> logger)
    {
        this.logger = logger;
    }

    public int Priority => int.MinValue; // always first stage
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (allowList.Contains(ev.EventId))
            // Event type is permitted, pass to the next stage of the pipeline.
            NextStage?.ProcessEvent(ev, connection);
        else
            // Event type is not permitted, log warning and drop the event.
            logger.LogWarning("Rejecting network event with ID {Id} not found on allowlist.", ev.EventId);
    }
}