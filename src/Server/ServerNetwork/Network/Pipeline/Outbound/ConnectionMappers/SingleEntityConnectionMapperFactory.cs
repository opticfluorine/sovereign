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
///     Factory class for creating SingleEntityConnectionMapper objects.
/// </summary>
/// <seealso cref="SingleEntityConnectionMapper" />
public class SingleEntityConnectionMapperFactory
{
    private readonly NetworkConnectionManager connectionManager;

    public SingleEntityConnectionMapperFactory(
        NetworkConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    /// <summary>
    ///     Creates a connection mapper.
    /// </summary>
    /// <param name="mapper">Function taking event info to a destination connection ID.</param>
    /// <returns>Connection mapper.</returns>
    public SingleEntityConnectionMapper Create(Func<OutboundEventInfo, Maybe<int>> mapper)
    {
        return new SingleEntityConnectionMapper(connectionManager, mapper);
    }
}