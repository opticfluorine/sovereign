// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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