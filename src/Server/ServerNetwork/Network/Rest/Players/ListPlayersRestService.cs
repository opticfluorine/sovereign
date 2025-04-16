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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for listing player characters.
/// </summary>
public class ListPlayersRestService : AuthenticatedRestService
{
    private readonly PersistencePlayerServices playerServices;

    public ListPlayersRestService(RestAuthenticator authenticator, PersistencePlayerServices playerServices,
        ILogger<ListPlayersRestService> logger) :
        base(authenticator, logger)
    {
        this.playerServices = playerServices;
    }

    public override string Path => RestEndpoints.Player;
    public override RestPathType PathType => RestPathType.Static;
    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        logger.LogDebug("ListPlayers requested for account ID {AccountId}.", accountId);
        try
        {
            var players = playerServices.GetPlayersForAccount(accountId);
            var response = new ListPlayersResponse { Players = players };
            var responseJson = JsonSerializer.Serialize(response);
            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while handling ListPlayers for account ID {Id}.", accountId);
            try
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send();
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Exception while sending error 500 during ListPlayers for account ID {Id}.",
                    accountId);
            }
        }
    }
}