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

using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Connection mapper that broadcasts events to all connected clients.
/// </summary>
public class GlobalConnectionMapper : ISpecificConnectionMapper
{
    private readonly NetworkConnectionManager connectionManager;

    public GlobalConnectionMapper(NetworkConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Broadcast the event info to all connections.
        connectionManager.Broadcast(conn => NextStage?.Process(new OutboundEventInfo(evInfo, conn)));
    }

    public IOutboundPipelineStage? NextStage { get; set; }
}