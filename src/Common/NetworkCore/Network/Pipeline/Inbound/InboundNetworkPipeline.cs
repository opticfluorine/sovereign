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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Inbound;

/// <summary>
///     Network pipeline that processes inbound network events.
/// </summary>
public sealed class InboundNetworkPipeline
{
    /// <summary>
    ///     First stage of the pipeline.
    /// </summary>
    private readonly IInboundPipelineStage firstStage;

    private readonly ILogger<InboundNetworkPipeline> logger;

    public InboundNetworkPipeline(IList<IInboundPipelineStage> stages, ILogger<InboundNetworkPipeline> logger)
    {
        this.logger = logger;

        /* Sort the stages and build the pipeline. */
        var sortedStages = stages.OrderBy(stage => stage.Priority).ToList();
        firstStage = sortedStages[0];
        var previous = firstStage;
        for (var i = 1; i < sortedStages.Count; ++i)
        {
            var current = sortedStages[i];
            previous.NextStage = current;
            previous = current;
        }
    }

    /// <summary>
    ///     Processes an inbound event.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// <param name="connection">Associated connection.</param>
    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        firstStage.ProcessEvent(ev, connection);
    }

    /// <summary>
    ///     Logs pipeline info at startup.
    /// </summary>
    public void OutputStartupDiagnostics()
    {
        var sb = new StringBuilder();
        sb.Append("Inbound network pipeline: ");
        var current = firstStage;
        while (current != null)
        {
            sb.Append(current.GetType().Name);
            current = current.NextStage;
            if (current != null)
                sb.Append(" -> ");
        }

        logger.LogDebug(sb.ToString());
    }
}