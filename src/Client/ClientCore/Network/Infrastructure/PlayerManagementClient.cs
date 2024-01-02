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
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Events;
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

    private readonly IEventSender eventSender;
    private readonly ClientNetworkInternalController internalController;

    private readonly RestClient restClient;

    public PlayerManagementClient(RestClient restClient, ClientNetworkInternalController internalController,
        IEventSender eventSender)
    {
        this.restClient = restClient;
        this.internalController = internalController;
        this.eventSender = eventSender;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<Option<ListPlayersResponse, string>> ListPlayersAsync()
    {
        var result = new Option<ListPlayersResponse, string>("Unexpected error occurred.");
        try
        {
            // REST client needs to be connected and authenticated.
            if (!restClient.Connected)
            {
                Logger.Error("Cannot create player while disconnected from server.");
                result = new Option<ListPlayersResponse, string>("Not connected.");
                return result;
            }

            // Send request.
            var httpResponse = await restClient.Get(RestEndpoints.Player);
            if (httpResponse.Content.Headers.ContentLength > MaxResponseLength)
            {
                Logger.ErrorFormat("ListPlayers response length {0} is too long.",
                    httpResponse.Content.Headers.ContentLength);
                result = new Option<ListPlayersResponse, string>("Response too long.");
                return result;
            }

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                Logger.ErrorFormat("ListPlayers response status {0}.", httpResponse.StatusCode);
                result = new Option<ListPlayersResponse, string>("Bad response from server.");
                return result;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<ListPlayersResponse>();
            result = response != null
                ? new Option<ListPlayersResponse, string>(response)
                : new Option<ListPlayersResponse, string>("Null response received.");
        }
        catch (Exception e)
        {
            Logger.Error("Exception while listing players.", e);
        }

        return result;
    }

    /// <summary>
    ///     Sends a request to the server to create a new player using the current account.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Response or error string.</returns>
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
            if (response == null)
            {
                var msg = "Received null response from server.";
                Logger.Error(msg);
                return new Option<CreatePlayerResponse, string>(msg);
            }

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                // Success
                Logger.InfoFormat("Successfully created new player {0} (ID {1}).", request.PlayerName,
                    response.PlayerId);
                internalController.SetPlayerEntityId(eventSender, response.PlayerId);
                result = new Option<CreatePlayerResponse, string>(response);
            }
            else
            {
                // Failed
                if (response.Result != null)
                {
                    Logger.ErrorFormat("Failed to create new player {0}: {1}", request.PlayerName, response.Result);
                    result = new Option<CreatePlayerResponse, string>(response.Result);
                }
                else
                {
                    Logger.ErrorFormat("Failed to create new player {0}: unknown error.", request.PlayerName);
                    result = new Option<CreatePlayerResponse, string>("Unknown error.");
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception thrown during player management.", e);
        }

        return result;
    }

    /// <summary>
    ///     Selects an existing player character for use in game.
    /// </summary>
    /// <param name="playerEntityId">PLayer entity ID.</param>
    /// <returns>Response or error string.</returns>
    public async Task<Option<SelectPlayerResponse, string>> SelectPlayerAsync(ulong playerEntityId)
    {
        var result = new Option<SelectPlayerResponse, string>("Unexpected error occurred.");

        try
        {
            // REST client needs to be connected and authenticated in order to
            // access player management APIs.
            if (!restClient.Connected)
            {
                Logger.Error("Cannot manage players while disconnected from server.");
                result = new Option<SelectPlayerResponse, string>("Not connected.");
                return result;
            }

            // Send the request.
            var playerUrl = new StringBuilder(RestEndpoints.Player).Append("/").Append(playerEntityId).ToString();
            var httpResponse = await restClient.Post(playerUrl);
            if (httpResponse.Content.Headers.ContentLength > MaxResponseLength)
            {
                Logger.ErrorFormat("SelectPlayer response length {0} is too long.",
                    httpResponse.Content.Headers.ContentLength);
                result = new Option<SelectPlayerResponse, string>("Response too long.");
                return result;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<SelectPlayerResponse>();
            if (response == null)
            {
                var msg = "Null response received from server.";
                Logger.Error(msg);
                return new Option<SelectPlayerResponse, string>(msg);
            }

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // Success
                Logger.InfoFormat("Selected player ID {0}.", playerEntityId);
                internalController.SetPlayerEntityId(eventSender, playerEntityId);
                result = new Option<SelectPlayerResponse, string>(response);
            }
            else
            {
                // Failed
                if (response.Result != null)
                {
                    Logger.ErrorFormat("Failed to select player {0}: {1}", playerEntityId, response.Result);
                    result = new Option<SelectPlayerResponse, string>(response.Result);
                }
                else
                {
                    Logger.ErrorFormat("Failed to select player {0}: Unknown error", playerEntityId);
                    result = new Option<SelectPlayerResponse, string>("Unknown error.");
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("Exception thrown during player management.", e);
        }

        return result;
    }
}