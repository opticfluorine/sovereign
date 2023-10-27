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