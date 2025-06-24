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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Player;
using Sovereign.ServerCore.Systems.Scripting;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST endpoint for providing ScriptInfo data.
/// </summary>
public class ScriptInfoRestService : AuthenticatedRestService
{
    private readonly AccountServices accountServices;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ScriptingServices scriptingServices;

    public ScriptInfoRestService(RestAuthenticator authenticator, ILogger<ScriptInfoRestService> logger,
        PlayerRoleCheck roleCheck, AccountServices accountServices, ScriptingServices scriptingServices)
        : base(authenticator, logger)
    {
        this.roleCheck = roleCheck;
        this.accountServices = accountServices;
        this.scriptingServices = scriptingServices;
    }

    public override string Path => RestEndpoints.ScriptInfo;
    public override RestPathType PathType => RestPathType.Static;
    public override HttpMethod RequestType => HttpMethod.GET;

    protected override async Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId)
    {
        if (!accountServices.TryGetPlayerForAccount(accountId, out var playerId) || !roleCheck.IsPlayerAdmin(playerId))
        {
            await SendAccessDenied(ctx);
            return;
        }

        var scriptInfo = scriptingServices.GetLoadedScriptInfo();
        var scriptInfoJson = JsonSerializer.Serialize(scriptInfo, MessageConfig.JsonOptions);

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.Send(scriptInfoJson);
    }

    /// <summary>
    ///     Sends an access denied response.
    /// </summary>
    /// <param name="ctx">Response context.</param>
    private async Task SendAccessDenied(HttpContextBase ctx)
    {
        ctx.Response.StatusCode = 403; // Forbidden
        await ctx.Response.Send("Access denied.");
    }
}