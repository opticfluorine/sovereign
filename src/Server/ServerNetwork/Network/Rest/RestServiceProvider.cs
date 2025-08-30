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

using Microsoft.AspNetCore.Builder;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerNetwork.Network.Rest.Accounts;
using Sovereign.ServerNetwork.Network.Rest.Players;
using Sovereign.ServerNetwork.Network.Rest.TemplateEntities;
using Sovereign.ServerNetwork.Network.Rest.WorldSegment;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Supports installing Sovereign's REST endpoints into an ASP.NET Core WebApplication.
/// </summary>
public sealed class RestServiceProvider(
    AuthenticationRestService authentication,
    AccountRegistrationRestService accountRegistration,
    ListPlayersRestService listPlayers,
    CreatePlayerRestService createPlayer,
    DeletePlayerRestService deletePlayer,
    SelectPlayerRestService selectPlayer,
    EntityDataRestService entityData,
    ScriptInfoRestService scriptInfo,
    SetTemplateEntityRestService setTemplateEntity,
    TemplateEntitiesRestService templateEntities,
    WorldSegmentRestService worldSegments)
{
    /// <summary>
    ///     Adds Sovereign endpoints to a WebApplication.
    /// </summary>
    /// <param name="app">Web application.</param>
    public void AddEndpoints(WebApplication app)
    {
        // Login and registration.
        app.MapPost(RestEndpoints.Authentication, authentication.PostLogin)
            .AllowAnonymous();
        app.MapPost(RestEndpoints.AccountRegistration, accountRegistration.PostRegister)
            .AllowAnonymous();

        // Player management.
        app.MapGet(RestEndpoints.Player, handler: listPlayers.ListPlayersGet)
            .RequireAuthorization();
        app.MapPost(RestEndpoints.Player, createPlayer.CreatePlayerPost)
            .RequireAuthorization(RestAuthorization.Policies.RequireOutOfGame);
        app.MapDelete($"{RestEndpoints.Player}/{{playerId}}", deletePlayer.PlayerDelete)
            .RequireAuthorization(RestAuthorization.Policies.RequireOutOfGame);
        app.MapPost($"{RestEndpoints.Player}/{{playerId}}", selectPlayer.SelectPlayerPost)
            .RequireAuthorization(RestAuthorization.Policies.RequireOutOfGame);

        // Template entities.
        app.MapGet(RestEndpoints.TemplateEntities, templateEntities.TemplateEntitiesGet);
        app.MapPost(RestEndpoints.TemplateEntities, setTemplateEntity.SetTemplateEntityPost)
            .RequireAuthorization(RestAuthorization.Policies.AdminOnly);
        app.MapGet(RestEndpoints.ScriptInfo, scriptInfo.ScriptInfoGet)
            .RequireAuthorization(RestAuthorization.Policies.AdminOnly);

        // Entity data.
        app.MapGet($"{RestEndpoints.EntityData}/{{entityId}}", entityData.EntityDataGet)
            .RequireAuthorization(RestAuthorization.Policies.AdminOnly);

        // World segments.
        app.MapGet($"{RestEndpoints.WorldSegment}/{{x}}/{{y}}/{{z}}", worldSegments.WorldSegmentGet)
            .RequireAuthorization(RestAuthorization.Policies.RequirePlayer);
    }
}