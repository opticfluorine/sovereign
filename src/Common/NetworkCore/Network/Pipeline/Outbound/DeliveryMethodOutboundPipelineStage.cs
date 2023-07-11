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
using LiteNetLib;
using Sovereign.EngineCore.Events;

namespace Sovereign.NetworkCore.Network.Pipeline.Outbound;

/// <summary>
///     Outbound pipeline stage responsible for selecting the delivery method of an event.
/// </summary>
public class DeliveryMethodOutboundPipelineStage : IOutboundPipelineStage
{
    /// <summary>
    ///     Map from event ID to delivery method.
    /// </summary>
    private readonly Dictionary<EventId, DeliveryMethod> methodMap = new()
    {
        { EventId.Core_Ping_Ping, DeliveryMethod.Unreliable },
        { EventId.Core_Ping_Pong, DeliveryMethod.Unreliable }
    };

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Next stage of the pipeline.
    /// </summary>
    public IOutboundPipelineStage NextStage { get; set; }

    public void Process(OutboundEventInfo evInfo)
    {
        var method = DeliveryMethod.Unreliable;
        if (methodMap.TryGetValue(evInfo.Event.EventId, out var mappedMethod))
            method = mappedMethod;
        else
            Logger.WarnFormat("No delivery method specified for event ID {0}; defaulting to unreliable",
                evInfo.Event.EventId);

        NextStage.Process(new OutboundEventInfo(evInfo, method));
    }
}