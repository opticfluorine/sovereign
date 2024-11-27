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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerNetwork.Network.Pipeline.Inbound;

/// <summary>
///     Server-side inbound network pipeline stage that allows for optional filtering of events to
///     only accept certain event IDs from certain players (e.g. restrict to admin).
/// </summary>
public class PlayerFilterInboundPipelineStage : IInboundPipelineStage
{
    private readonly AccountServices accountServices;
    private readonly ILogger<PlayerFilterInboundPipelineStage> logger;
    private readonly LoggingUtil loggingUtil;

    private readonly Dictionary<EventId, Func<ulong, bool>> mappers;
    private readonly PlayerRoleCheck roleCheck;

    public PlayerFilterInboundPipelineStage(AccountServices accountServices, PlayerRoleCheck roleCheck,
        LoggingUtil loggingUtil, ILogger<PlayerFilterInboundPipelineStage> logger)
    {
        this.accountServices = accountServices;
        this.roleCheck = roleCheck;
        this.loggingUtil = loggingUtil;
        this.logger = logger;

        mappers = new Dictionary<EventId, Func<ulong, bool>>
        {
            { EventId.Server_WorldEdit_SetBlock, RequireAdmin },
            { EventId.Server_WorldEdit_RemoveBlock, RequireAdmin }
        };
    }

    public int Priority => 50;
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (mappers.TryGetValue(ev.EventId, out var checkFunction))
        {
            var playerEntityId = accountServices.GetPlayerForConnectionId(connection.Id);
            if (!playerEntityId.HasValue || !checkFunction.Invoke(playerEntityId.Value)) return;
        }

        NextStage?.ProcessEvent(ev, connection);
    }

    /// <summary>
    ///     Check function that requires the player to be an admin.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns></returns>
    private bool RequireAdmin(ulong playerEntityId)
    {
        if (!roleCheck.IsPlayerAdmin(playerEntityId))
        {
            logger.LogWarning("Admin-only event rejected for non-admin player {Name}.",
                loggingUtil.FormatEntity(playerEntityId));
            return false;
        }

        return true;
    }
}