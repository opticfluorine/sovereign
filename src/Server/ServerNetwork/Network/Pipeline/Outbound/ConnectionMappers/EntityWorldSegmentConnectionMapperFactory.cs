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
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Factory class for EntityWorldSegmentConnectionMapper.
/// </summary>
public class EntityWorldSegmentConnectionMapperFactory
{
    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ILogger<EntityWorldSegmentConnectionMapperFactory> logger;
    private readonly WorldSegmentResolver resolver;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ServerOnlyTagCollection serverOnly;
    private readonly WorldManagementServices worldManagementServices;

    public EntityWorldSegmentConnectionMapperFactory(
        KinematicsComponentCollection kinematics,
        WorldSegmentResolver resolver,
        WorldManagementServices worldManagementServices,
        NetworkConnectionManager connectionManager,
        AccountServices accountServices,
        ServerOnlyTagCollection serverOnly,
        PlayerRoleCheck roleCheck,
        ILogger<EntityWorldSegmentConnectionMapperFactory> logger)
    {
        this.kinematics = kinematics;
        this.resolver = resolver;
        this.worldManagementServices = worldManagementServices;
        this.connectionManager = connectionManager;
        this.accountServices = accountServices;
        this.serverOnly = serverOnly;
        this.roleCheck = roleCheck;
        this.logger = logger;
    }

    /// <summary>
    ///     Creates an entity-based world segment resolver.
    /// </summary>
    /// <param name="entitySelector">Function that gets the entity ID for the event.</param>
    /// <returns>Mapper.</returns>
    public EntityWorldSegmentConnectionMapper Create(
        Func<OutboundEventInfo, Maybe<ulong>> entitySelector)
    {
        return new EntityWorldSegmentConnectionMapper(
            kinematics, resolver, worldManagementServices, connectionManager,
            accountServices, serverOnly, roleCheck, logger, entitySelector);
    }
}