// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.ServerNetwork.Systems.Network;

namespace Sovereign.ServerNetwork.Network.Pipeline.Inbound;

/// <summary>
///     Inbound pipeline stage that flags selected client-sent events as local so that they are
///     both dispatched to subscribing systems and rebroadcast by the outbound pipeline.
/// </summary>
public class RebroadcastedInboundPipelineStage(ServerRebroadcastedEventSet rebroadcastedEventSet)
    : IInboundPipelineStage
{
    public int Priority => 200;
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (rebroadcastedEventSet.EventIds.Contains(ev.EventId)) ev.Local = true;

        NextStage?.ProcessEvent(ev, connection);
    }
}
