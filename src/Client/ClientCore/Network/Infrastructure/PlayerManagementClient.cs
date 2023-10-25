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
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Provides client-side access to the server's player management API
///     for listing/creating/deleting/selecting player characters.
/// </summary>
public class PlayerManagementClient
{
    /// <summary>
    ///     Maximum response length, in bytes.
    /// </summary>
    private const int MaxResponseLength = 1024;

    private readonly RestClient restClient;

    public PlayerManagementClient(RestClient restClient)
    {
        this.restClient = restClient;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Option<CreatePlayerResponse, string>> CreatePlayerAsync(
        CreatePlayerRequest request)
    {
        var result = new Option<CreatePlayerResponse, string>("Unexpected error occurred.");

        try
        {
            // REST client needs to be connected and authenticated in order to
            // access player management APIs.
            if (!restClient.Connected)
            {
                Logger.Error("Cannot manage players while disconnected from server.");
                result = new Option<CreatePlayerResponse, string>("Not connected.");
                return result;
            }

            // Send the request.
            var httpResponse = await restClient.PostJson(RestEndpoints.Player, request);
            if (httpResponse.Content.Headers.ContentLength > MaxResponseLength)
            {
                Logger.ErrorFormat("CreatePlayer response length {0} is too long.",
                    httpResponse.Content.Headers.ContentLength);
                result = new Option<CreatePlayerResponse, string>("Response too long.");
                return result;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<CreatePlayerResponse>();
            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                // Success
                Logger.InfoFormat("Successfully created new player {0} (ID {1}).", request.PlayerName,
                    response.PlayerId);
                result = new Option<CreatePlayerResponse, string>(response);
            }
            else
            {
                // Failed
                Logger.ErrorFormat("Failed to create new player {0}: {1}", request.PlayerName, response.Result);
                result = new Option<CreatePlayerResponse, string>(response.Result);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception thrown during player management.", e);
        }

        return result;
    }
}