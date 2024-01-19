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
        { EventId.Core_Ping_Pong, DeliveryMethod.Unreliable },
        { EventId.Core_WorldManagement_Subscribe, DeliveryMethod.ReliableUnordered },
        { EventId.Core_WorldManagement_Unsubscribe, DeliveryMethod.ReliableUnordered },
        { EventId.Client_EntitySynchronization_Update, DeliveryMethod.ReliableUnordered },
        { EventId.Core_Movement_Move, DeliveryMethod.ReliableUnordered },
        { EventId.Core_Movement_RequestMove, DeliveryMethod.ReliableUnordered }
    };

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Next stage of the pipeline.
    /// </summary>
    public IOutboundPipelineStage? NextStage { get; set; }

    public void Process(OutboundEventInfo evInfo)
    {
        var method = DeliveryMethod.Unreliable;
        if (methodMap.TryGetValue(evInfo.Event.EventId, out var mappedMethod))
            method = mappedMethod;
        else
            Logger.WarnFormat("No delivery method specified for event ID {0}; defaulting to unreliable",
                evInfo.Event.EventId);

        NextStage?.Process(new OutboundEventInfo(evInfo, method));
    }
}