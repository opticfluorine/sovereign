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

using Castle.Core.Logging;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Outbound;

public class FinalOutboundPipelineStage : IOutboundPipelineStage
{
    private readonly INetworkManager networkManager;

    public FinalOutboundPipelineStage(INetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Process(OutboundEventInfo evInfo)
    {
        // Validity check.
        if (evInfo.Event == null || evInfo.Connection == null)
        {
            Logger.Warn("Incomplete event info produced by output pipeline; discarding.");
            return;
        }

        // Enqueue the event to be sent over the network.
        networkManager.EnqueueEvent(evInfo);
    }
}