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
using Microsoft.Extensions.Logging;
using WatsonWebserver.Core;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Reusable implementation of IRestService that provides for transparent authentication of REST requests.
/// </summary>
public abstract class AuthenticatedRestService : IRestService
{
    private readonly RestAuthenticator authenticator;
    protected readonly ILogger logger;

    public AuthenticatedRestService(RestAuthenticator authenticator, ILogger logger)
    {
        this.authenticator = authenticator;
        this.logger = logger;
    }

    public abstract string Path { get; }
    public abstract RestPathType PathType { get; }
    public abstract HttpMethod RequestType { get; }

    public async Task OnRequest(HttpContextBase ctx)
    {
        switch (authenticator.Authenticate(ctx, out var accountId))
        {
            case RestAuthenticationResult.Success:
                await OnAuthenticatedRequest(ctx, accountId.Value);
                break;

            case RestAuthenticationResult.MissingCredentials:
                authenticator.SetupUnauthorizedResponse(ctx.Response);
                await ctx.Response.Send();
                break;

            case RestAuthenticationResult.Denied:
            default:
                logger.LogInformation("403 Forbidden - {Ip} for {Url}", ctx.Request.Source.IpAddress,
                    ctx.Request.Url.Full);
                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
                break;
        }
    }

    /// <summary>
    ///     Called when a request is received and is successfully authenticated.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <param name="accountId">Authenticated account ID associated with the request.</param>
    /// <returns>Task for further processing of the response.</returns>
    protected abstract Task OnAuthenticatedRequest(HttpContextBase ctx, Guid accountId);
}