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
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound;

/// <summary>
///     Maps events to connections in the server-side outbound pipeline.
/// </summary>
public class ServerConnectionMappingOutboundPipelineStage : IConnectionMappingOutboundPipelineStage
{
    /// <summary>
    ///     Map of outbound events to specialized connection mappers.
    /// </summary>
    private readonly Dictionary<EventId, ISpecificConnectionMapper> specificMappers = new();

    private IOutboundPipelineStage nextStage;

    public ServerConnectionMappingOutboundPipelineStage(GlobalConnectionMapper globalMapper)
    {
        // Configure specific connection mappers.
        specificMappers[EventId.Core_Ping_Ping] = globalMapper;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Process(OutboundEventInfo evInfo)
    {
        // Dispatch to the correct mapping strategy.
        if (specificMappers.TryGetValue(evInfo.Event.EventId, out var mapper))
            mapper.Process(evInfo);
        else
            // No mapper found for this event type.
            Logger.ErrorFormat("No connection mapper available for event ID {0}.", evInfo.Event.EventId);
    }

    public IOutboundPipelineStage NextStage
    {
        get => nextStage;
        set
        {
            nextStage = value;
            foreach (var mapper in specificMappers.Values) mapper.NextStage = nextStage;
        }
    }
}