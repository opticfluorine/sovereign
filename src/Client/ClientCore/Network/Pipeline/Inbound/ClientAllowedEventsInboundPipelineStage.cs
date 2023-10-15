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
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;

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
        EventId.Core_Ping_Ping
    };

    public ILogger Logger { private get; set; } = NullLogger.Instance;
    public int Priority => int.MinValue; // always first stage
    public IInboundPipelineStage NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (allowList.Contains(ev.EventId))
            // Event type is permitted, pass to the next stage of the pipeline.
            NextStage.ProcessEvent(ev, connection);
        else
            // Event type is not permitted, log warning and drop the event.
            Logger.WarnFormat("Rejecting network event with ID {0} not found on allowlist.", ev.EventId);
    }
}