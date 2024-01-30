// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Factory class for WorldSegmentConnectionMapper.
/// </summary>
public class WorldSegmentConnectionMapperFactory
{
    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly WorldManagementServices worldManagementServices;

    public WorldSegmentConnectionMapperFactory(WorldManagementServices worldManagementServices,
        AccountServices accountServices, NetworkConnectionManager connectionManager)
    {
        this.worldManagementServices = worldManagementServices;
        this.accountServices = accountServices;
        this.connectionManager = connectionManager;
    }

    /// <summary>
    ///     Creates a new WorldSegmentConnectionMapper.
    /// </summary>
    /// <param name="mapper">Function taking an outbound event to its associated world segment index.</param>
    /// <returns>Connection mapper.</returns>
    public WorldSegmentConnectionMapper Create(Func<OutboundEventInfo, Maybe<GridPosition>> mapper)
    {
        return new WorldSegmentConnectionMapper(worldManagementServices, accountServices,
            connectionManager, mapper);
    }
}