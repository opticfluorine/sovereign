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

using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Events.Details;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;

/// <summary>
///     Connection mapper for chat messages with variable target types.
/// </summary>
public class GenericChatConnectionMapper : ISpecificConnectionMapper
{
    private readonly AccountServices accountServices;
    private readonly NetworkConnectionManager connectionManager;
    private readonly ILogger<GenericChatConnectionMapper> logger;

    public GenericChatConnectionMapper(NetworkConnectionManager connectionManager,
        AccountServices accountServices, ILogger<GenericChatConnectionMapper> logger)
    {
        this.connectionManager = connectionManager;
        this.accountServices = accountServices;
        this.logger = logger;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        if (evInfo.Event.EventDetails is not GenericChatEventDetails details)
        {
            logger.LogError("Event has invalid details for this mapper.");
            return;
        }

        switch (details.Target)
        {
            case GenericChatTarget.Player:
                var connId = accountServices.GetConnectionIdForPlayer(details.TargetId);
                if (!connId.HasValue)
                {
                    logger.LogError("No connection ID found for player.");
                    return;
                }

                NextStage?.Process(new OutboundEventInfo(evInfo, connectionManager.GetConnection(connId.Value)));
                break;

            case GenericChatTarget.Global:
                connectionManager.Broadcast(conn => NextStage?.Process(new OutboundEventInfo(evInfo, conn)));
                break;

            default:
                logger.LogError("Unknown chat target.");
                break;
        }
    }

    public IOutboundPipelineStage? NextStage { get; set; }
}