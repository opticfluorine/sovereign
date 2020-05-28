/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sovereign.NetworkCore.Network.Pipeline
{

    /// <summary>
    /// Network pipeline that processes outbound network events.
    /// </summary>
    public sealed class OutboundNetworkPipeline
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// First stage of the pipeline.
        /// </summary>
        private readonly IOutboundPipelineStage firstStage;

        public OutboundNetworkPipeline(IList<IOutboundPipelineStage> stages)
        {
            /* Sort the stages and build the pipeline. */
            var sortedStages = stages.OrderBy((stage) => stage.Priority).ToList();
            firstStage = sortedStages[0];
            var previous = firstStage;
            for (int i = 1; i < sortedStages.Count; ++i)
            {
                var current = sortedStages[i];
                previous.NextStage = current;
                previous = current;
            }
        }

        /// <summary>
        /// Processes an outbound event.
        /// </summary>
        /// <param name="ev">Event.</param>
        /// <param name="connection">Associated connection.</param>
        public void ProcessEvent(Event ev, NetworkConnection connection)
        {
            firstStage.ProcessEvent(ev, connection);
        }

        /// <summary>
        /// Logs pipeline info at startup.
        /// </summary>
        public void OutputStartupDiagnostics()
        {
            var sb = new StringBuilder();
            sb.Append("Outbound network pipeline: ");
            var current = firstStage;
            while (current != null)
            {
                sb.Append(current.GetType().Name);
                current = current.NextStage;
                if (current != null)
                    sb.Append(" -> ");
            }
            Logger.Debug(sb.ToString());
        }

    }

}
