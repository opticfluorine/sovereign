// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineUtil.Monads;
using Sovereign.WorldGen;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     REST client for saving world generation profiles on the server.
/// </summary>
public sealed class WorldGenProfileClient
{
    /// <summary>
    ///     Maximum length of an error response body, in characters.
    /// </summary>
    private const int MaxErrorLength = 65536;

    private readonly ILogger<WorldGenProfileClient> logger;
    private readonly RestClient restClient;

    public WorldGenProfileClient(RestClient restClient, ILogger<WorldGenProfileClient> logger)
    {
        this.restClient = restClient;
        this.logger = logger;
    }

    /// <summary>
    ///     Saves a world generation profile on the server, overwriting any existing profile
    ///     with the same name. Requires an administrator account.
    /// </summary>
    /// <param name="name">Profile name, without extension.</param>
    /// <param name="profile">Profile to save.</param>
    /// <returns>Option with true on success, or a human-readable error description.</returns>
    public async Task<Option<bool, string>> SaveProfileAsync(string name, WorldGenProfile profile)
    {
        try
        {
            // REST client needs to be connected and authenticated.
            if (!restClient.Connected)
            {
                logger.LogError("Cannot save world generation profile while disconnected from server.");
                return new Option<bool, string>("Not connected.");
            }

            // Send the request.
            var url = $"{RestEndpoints.WorldGenProfiles}/{Uri.EscapeDataString(name)}";
            var httpResponse = await restClient.PutJson(url, profile);

            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                return new Option<bool, string>(true);
            }

            if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                // The rejection reason is reported in the response body.
                var detail = await httpResponse.Content.ReadAsStringAsync();
                if (detail.Length > MaxErrorLength) detail = detail[..MaxErrorLength];
                logger.LogError("Server rejected world generation profile \"{Name}\": {Detail}",
                    name, detail);
                return new Option<bool, string>(
                    string.IsNullOrWhiteSpace(detail) ? "The profile was rejected." : detail);
            }

            if (httpResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                logger.LogError("Not authorized to save world generation profile \"{Name}\".", name);
                return new Option<bool, string>("Not authorized.");
            }

            logger.LogError("SaveProfile response status {StatusCode}.", httpResponse.StatusCode);
            return new Option<bool, string>("Bad response from server.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while saving world generation profile.");
            return new Option<bool, string>("Unexpected error occurred.");
        }
    }
}
