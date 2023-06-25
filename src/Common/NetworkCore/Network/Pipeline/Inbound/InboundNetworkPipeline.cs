/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Inbound
{

    /// <summary>
    /// Network pipeline that processes inbound network events.
    /// </summary>
    public sealed class InboundNetworkPipeline
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// First stage of the pipeline.
        /// </summary>
        private readonly IInboundPipelineStage firstStage;

        public InboundNetworkPipeline(IList<IInboundPipelineStage> stages)
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
        /// Processes an inbound event.
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
            sb.Append("Inbound network pipeline: ");
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
