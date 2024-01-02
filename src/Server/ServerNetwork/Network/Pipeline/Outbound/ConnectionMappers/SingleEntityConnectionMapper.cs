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

using System;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Connection mapper that maps an event onto a single network connection.
/// </summary>
public class SingleEntityConnectionMapper : ISpecificConnectionMapper
{
    private readonly NetworkConnectionManager connectionManager;
    private readonly Func<OutboundEventInfo, Maybe<int>> mapper;

    /// <summary>
    ///     Creates a new connection mapper.
    /// </summary>
    /// <param name="connectionManager">Connection manager.</param>
    /// <param name="mapper">Function taking event info to a connection ID.</param>
    /// <seealso cref="SingleEntityConnectionMapperFactory" />
    internal SingleEntityConnectionMapper(
        NetworkConnectionManager connectionManager,
        Func<OutboundEventInfo, Maybe<int>> mapper)
    {
        this.connectionManager = connectionManager;
        this.mapper = mapper;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        var connId = mapper.Invoke(evInfo);
        if (connId.HasValue)
        {
            var connection = connectionManager.GetConnection(connId.Value);
            NextStage?.Process(new OutboundEventInfo(evInfo, connection));
        }
    }

    public IOutboundPipelineStage? NextStage { get; set; }
}