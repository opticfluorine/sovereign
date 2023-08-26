// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Reusable implementation of IRestService that provides for transparent authentication of REST requests.
/// </summary>
public abstract class AuthenticatedRestService : IRestService
{
    private readonly RestAuthenticator authenticator;

    public AuthenticatedRestService(RestAuthenticator authenticator)
    {
        this.authenticator = authenticator;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public abstract string Path { get; }
    public abstract RestPathType PathType { get; }
    public abstract HttpMethod RequestType { get; }

    public async Task OnRequest(HttpContext ctx)
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
                Logger.InfoFormat("403 Forbidden - {0} for {1}", ctx.Request.Source.IpAddress,
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
    protected abstract Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId);
}