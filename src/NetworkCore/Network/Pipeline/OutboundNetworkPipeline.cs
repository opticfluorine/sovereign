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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
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
        public void ProcessEvent(Event ev)
        {
            firstStage.ProcessEvent(ev);
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
