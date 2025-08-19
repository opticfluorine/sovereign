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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for listing player characters.
/// </summary>
public class ListPlayersRestService
{
    private readonly ILogger<ListPlayersRestService> logger;
    private readonly PersistencePlayerServices playerServices;

    public ListPlayersRestService(PersistencePlayerServices playerServices, ILogger<ListPlayersRestService> logger)
    {
        this.playerServices = playerServices;
        this.logger = logger;
    }

    /// <summary>
    ///     GET endpoint for listing players belonging to the logged in account.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> ListPlayersGet(HttpContext context)
    {
        try
        {
            var accountId = Guid.Parse(context.User.FindFirst(RestAuthentication.ClaimTypes.AccountId)?.Value ??
                                       throw new Exception("User is missing AccountId claim."));
            var players = playerServices.GetPlayersForAccount(accountId);
            return Task.FromResult(Results.Ok(new ListPlayersResponse { Players = players }));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while handling ListPlayers. (Request ID: {Id})", context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError());
        }
    }
}