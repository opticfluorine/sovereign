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

using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.ClientCore.Network.Pipeline.Outbound;

/// <summary>
///     Maps events to connections in the client-side outbound pipeline.
/// </summary>
public class ClientConnectionMappingOutboundPipelineStage : IConnectionMappingOutboundPipelineStage
{
    private readonly INetworkClient client;

    public ClientConnectionMappingOutboundPipelineStage(INetworkClient client)
    {
        this.client = client;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // If the client is connected, associate the event to the connection and proceed.
        if (client.ClientState == NetworkClientState.Connected)
            NextStage.Process(new OutboundEventInfo(evInfo, client.Connection));
    }

    public IOutboundPipelineStage NextStage { get; set; }
}