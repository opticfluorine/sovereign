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
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     REST service for creating new player characters.
/// </summary>
public class CreatePlayerRestService : AuthenticatedRestService
{
    /// <summary>
    ///     Maximum request length, in bytes.
    /// </summary>
    private const int MaxRequestLength = 1024;

    /// <summary>
    ///     Object used as a lock to avoid name duplication due to a race condition.
    /// </summary>
    private readonly object creationLock = new();

    private readonly IEntityFactory entityFactory;

    /// <summary>
    ///     Tracks which names have been used recently in order to avoid duplication.
    /// </summary>
    private readonly HashSet<string> recentNames = new();

    public CreatePlayerRestService(RestAuthenticator authenticator, IEntityFactory entityFactory) : base(authenticator)
    {
        this.entityFactory = entityFactory;
    }

    public override string Path => RestEndpoints.Player + "/{id}";
    public override RestPathType PathType => RestPathType.Parameter;
    public override HttpMethod RequestType => HttpMethod.POST;

    protected override async Task OnAuthenticatedRequest(HttpContext ctx, Guid accountId)
    {
        try
        {
            // Safety check.
            if (ctx.Request.ContentLength > MaxRequestLength)
            {
                await SendResponse(ctx, 413, "Request too large.");
                return;
            }

            // Decode and validate input.
            var requestJson = ctx.Request.DataAsString;
            var requestData = JsonSerializer.Deserialize<CreatePlayerRequest>(requestJson,
                MessageConfig.JsonOptions);
            if (requestData.PlayerName == null)
            {
                await SendResponse(ctx, 400, "Incomplete input.");
                return;
            }

            // Attempt player creation.
            // TODO
        }
        catch (Exception e)
        {
            Logger.Error("Error handling create player request.", e);
            try
            {
                await SendResponse(ctx, 500, "Error processing request.");
            }
            catch (Exception e2)
            {
                Logger.Error("Error sending error response.", e2);
            }
        }

        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sends a response.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <param name="status">Response status code.</param>
    /// <param name="result">Human-readable string describing the result.</param>
    private async Task SendResponse(HttpContext ctx, int status, string result)
    {
        ctx.Response.StatusCode = status;

        var responseData = new CreatePlayerResponse
        {
            Result = result
        };
        var responseJson = JsonSerializer.Serialize(responseData);

        await ctx.Response.Send(Encoding.UTF8.GetBytes(responseJson));
    }

    /// <summary>
    ///     Attempts to create a new player character.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>true on success, false otherwise.</returns>
    private bool CreatePlayer(CreatePlayerRequest request)
    {
        // Lock to avoid creating new player characters in parallel.
        // The persistence scheme for the entity-component-system implementation forces a tradeoff
        // between good component design and the ability to enforce a unique player name constraint
        // in the database. To enforce a unique player name constraint, the player name component
        // would have to be its own component separate from Name - not ideal. Instead we opt to
        // not enforce a constraint in the database, and instead use a mutex to ensure that we don't
        // have a race condition leading to the creation of multiple player characters with the smae
        // name.
        var result = false;
        lock (creationLock)
        {
            if (!recentNames.Contains(request.PlayerName) && NameAvailable(request.PlayerName))
            {
                // Name is available and wasn't created recently - let's take it.
                entityFactory.GetBuilder()
                    .Name(request.PlayerName)
                    .PlayerCharacter()
                    // TODO Default starting position
                    .Build();

                recentNames.Add(request.PlayerName);
                result = true;
            }
            else
            {
                // Name already taken.
                result = false;
            }
        }

        return result;
    }

    /// <summary>
    ///     Checks whether the given player name is currently available.
    /// </summary>
    /// <param name="requestPlayerName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool NameAvailable(string requestPlayerName)
    {
        throw new NotImplementedException();
    }
}