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
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Player;
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.ServerCore.Systems.TemplateEntity;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST endpoint for creating or updating template entities.
/// </summary>
public class SetTemplateEntityRestService : AuthenticatedRestService
{
    private const int MaxContenLength = 65536; // 64 KB
    private readonly AccountServices accountServices;
    private readonly EntityDefinitionValidator definitionValidator;
    private readonly IEventSender eventSender;
    private readonly LoggingUtil loggingUtil;
    private readonly PlayerRoleCheck roleCheck;
    private readonly TemplateEntityController templateEntityController;

    public SetTemplateEntityRestService(RestAuthenticator authenticator, ILogger<SetTemplateEntityRestService> logger,
        AccountServices accountServices, PlayerRoleCheck roleCheck, LoggingUtil loggingUtil,
        TemplateEntityController templateEntityController, IEventSender eventSender,
        EntityDefinitionValidator definitionValidator) :
        base(authenticator, logger)
    {
        this.accountServices = accountServices;
        this.roleCheck = roleCheck;
        this.loggingUtil = loggingUtil;
        this.templateEntityController = templateEntityController;
        this.eventSender = eventSender;
        this.definitionValidator = definitionValidator;
    }

    public override string Path => RestEndpoints.TemplateEntities + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.POST;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        try
        {
            var hasPlayer = accountServices.TryGetPlayerForAccount(accountId, out var playerId);
            if (!hasPlayer || !roleCheck.IsPlayerAdmin(playerId))
            {
                if (hasPlayer)
                    logger.LogError("403 Forbidden - Access denied for player {PlayerName}.",
                        loggingUtil.FormatEntity(playerId));
                else
                    logger.LogError("403 Forbidden - Access denied for account {AccountId}.", accountId);

                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
                return;
            }

            if (ctx.Request.ContentLength > MaxContenLength)
            {
                ctx.Response.StatusCode = 413; // Payload Too Large
                await ctx.Response.Send();
                return;
            }

            SetTemplateEntityRequest request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<SetTemplateEntityRequest>(ctx.Request.Data,
                              MessageConfig.JsonOptionsWithFields)
                          ?? throw new Exception("Request body is empty.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Bad request from player {PlayerName}.", loggingUtil.FormatEntity(playerId));
                ctx.Response.StatusCode = 400; // Bad Request
                await ctx.Response.Send();
                return;
            }

            if (!definitionValidator.Validate(request.EntityDefinition))
            {
                logger.LogError("Invalid definition from player {PlayerName}.", loggingUtil.FormatEntity(playerId));
                ctx.Response.StatusCode = 400; // Bad Request
                await ctx.Response.Send();
                return;
            }

            if (!ulong.TryParse(ctx.Request.Url.Parameters["id"], NumberStyles.HexNumber, null, out var templateId) ||
                templateId != request.EntityDefinition.EntityId)
            {
                logger.LogError("Template entity ID mismatch from player {PlayerName}.",
                    loggingUtil.FormatEntity(playerId));
                ctx.Response.StatusCode = 400; // Bad Request
                await ctx.Response.Send();
                return;
            }

            // All keys and values are valid for entity data, so no need to validate the dictionary.
            // If we get here, the request is valid and we can process the update.
            templateEntityController.UpdateTemplateEntity(eventSender, request.EntityDefinition,
                request.EntityKeyValuePairs);

            ctx.Response.StatusCode = 200; // OK
            await ctx.Response.Send();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing SetTemplateEntity request for account {AccountId}.", accountId);
            ctx.Response.StatusCode = 500; // Internal Server Error
            try
            {
                await ctx.Response.Send();
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Error sending error response.");
            }
        }
    }
}