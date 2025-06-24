// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineUtil.Collections;
using Sovereign.NetworkCore.Network.Rest.Data;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

public class EntityDataRestService : AuthenticatedRestService
{
    private readonly AccountServices accountServices;
    private readonly ObjectPool<Dictionary<string, string>> dataPool = new();
    private readonly IDataServices dataServices;
    private readonly LoggingUtil loggingUtil;
    private readonly PlayerRoleCheck roleCheck;

    public EntityDataRestService(RestAuthenticator authenticator, ILogger<EntityDataRestService> logger,
        AccountServices accountServices, PlayerRoleCheck roleCheck, LoggingUtil loggingUtil, IDataServices dataServices)
        : base(authenticator, logger)
    {
        this.accountServices = accountServices;
        this.roleCheck = roleCheck;
        this.loggingUtil = loggingUtil;
        this.dataServices = dataServices;
    }

    public override string Path => RestEndpoints.EntityData + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        try
        {
            var hasPlayer = accountServices.TryGetPlayerForAccount(accountId, out var playerId);
            if (!hasPlayer || !roleCheck.IsPlayerAdmin(playerId))
            {
                if (hasPlayer)
                    logger.LogError("403 Forbidden - Bad access attempt by player {Player}.",
                        loggingUtil.FormatEntity(playerId));
                else
                    logger.LogError("403 Forbidden - Bad access attempt by account {AccountId}.", accountId);

                ctx.Response.StatusCode = 403; // Forbidden
                await ctx.Response.Send();
                return;
            }

            if (!ulong.TryParse(ctx.Request.Url.Parameters["id"], NumberStyles.HexNumber, null, out var entityId))
            {
                logger.LogError("Bad entity ID {EntityId} from player {Player}.", ctx.Request.Url.Parameters["id"],
                    loggingUtil.FormatEntity(playerId));
                ctx.Response.StatusCode = 400; // Bad Request
                await ctx.Response.Send();
                return;
            }

            Dictionary<string, string> data = dataPool.TakeObject();
            var responseJson = string.Empty;
            try
            {
                dataServices.GetEntityData(entityId, data);
                var response = new EntityDataResponse
                {
                    EntityData = data
                };
                responseJson = JsonSerializer.Serialize(response, MessageConfig.JsonOptions);
            }
            finally
            {
                dataPool.ReturnObject(data);
            }

            ctx.Response.StatusCode = 200; // OK
            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(responseJson);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing entity data request for entity ID {EntityId} and account {AccountId}.",
                ctx.Request.Url.Parameters["id"] ?? "[None]", accountId);
            ctx.Response.StatusCode = 500; // Internal Server Error
            try
            {
                await ctx.Response.Send();
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Error sending response for entity data request.");
            }
        }
    }
}