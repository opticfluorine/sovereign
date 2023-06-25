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

using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Inbound
{

    /// <summary>
    /// Interface to an individual stage in the inbound network pipeline.
    /// </summary>
    /// <remarks>
    /// Inbound pipeline stages may accept or reject outbound events as well
    /// as modify them. If an outbound event is accepted, it is passed to the
    /// next stage of the pipeline after applying any transformation. An event
    /// may be rejected simply by not passing anything to the next stage of
    /// the pipeline.
    /// </remarks>
    public interface IInboundPipelineStage
    {

        /// <summary>
        /// Stage priority. Lower priorities are executed earlier. The ordering
        /// of stages having equal priority is undefined.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Next stage in the inbound network pipeline.
        /// </summary>
        IInboundPipelineStage NextStage { get; set; }

        /// <summary>
        /// Processes an inbound event.
        /// </summary>
        /// <param name="ev">Inbound event.</param>
        /// <param name="connection">Associated connection.</param>
        void ProcessEvent(Event ev, NetworkConnection connection);

    }

}
