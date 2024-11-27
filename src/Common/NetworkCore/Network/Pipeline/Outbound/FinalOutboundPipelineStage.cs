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

using Microsoft.Extensions.Logging;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Outbound;

public class FinalOutboundPipelineStage : IOutboundPipelineStage
{
    private readonly ILogger<FinalOutboundPipelineStage> logger;
    private readonly INetworkManager networkManager;

    public FinalOutboundPipelineStage(INetworkManager networkManager, ILogger<FinalOutboundPipelineStage> logger)
    {
        this.networkManager = networkManager;
        this.logger = logger;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Validity check.
        if (evInfo.Event == null || evInfo.Connection == null)
        {
            logger.LogWarning("Incomplete event info produced by output pipeline; discarding.");
            return;
        }

        // Enqueue the event to be sent over the network.
        networkManager.EnqueueEvent(evInfo);
    }
}