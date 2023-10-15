/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;

namespace Sovereign.NetworkCore.Network.Pipeline.Outbound;

/// <summary>
///     Network pipeline that processes outbound network events.
/// </summary>
public sealed class OutboundNetworkPipeline
{
    private readonly IConnectionMappingOutboundPipelineStage connectionMappingStage;
    private readonly DeliveryMethodOutboundPipelineStage deliveryMappingStage;
    private readonly FinalOutboundPipelineStage finalStage;

    private readonly IOutboundPipelineStage firstStage;

    public OutboundNetworkPipeline(IConnectionMappingOutboundPipelineStage connectionMappingStage,
        DeliveryMethodOutboundPipelineStage deliveryMappingStage,
        FinalOutboundPipelineStage finalStage)
    {
        // Dependency injection.
        this.deliveryMappingStage = deliveryMappingStage;
        this.connectionMappingStage = connectionMappingStage;
        this.finalStage = finalStage;

        // Wire up the stages.
        deliveryMappingStage.NextStage = connectionMappingStage;
        connectionMappingStage.NextStage = finalStage;
        firstStage = deliveryMappingStage;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Processes an outbound event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void ProcessEvent(Event ev)
    {
        firstStage.Process(new OutboundEventInfo(ev));
    }
}