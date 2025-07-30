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
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     REST client for retrieving script information from the server.
/// </summary>
public class ScriptInfoClient
{
    private const int MaxContentLength = 65536; // 64 KB
    private readonly ILogger<ScriptInfoClient> logger;
    private readonly RestClient restClient;

    public ScriptInfoClient(RestClient restClient, ILogger<ScriptInfoClient> logger)
    {
        this.restClient = restClient;
        this.logger = logger;
    }

    /// <summary>
    ///     Current ScriptInfo object.
    /// </summary>
    public ScriptInfo ScriptInfo { get; private set; } = new();

    /// <summary>
    ///     Retrieves the latest ScriptInfo from the server asynchronously.
    /// </summary>
    public void FetchScriptInfo()
    {
        Task.Run(async () =>
        {
            try
            {
                var response = await restClient.Get(RestEndpoints.ScriptInfo);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError("Failed to fetch script info from server. Status code: {StatusCode}",
                        response.StatusCode);
                    return;
                }

                if (response.Content.Headers.ContentLength > MaxContentLength)
                {
                    logger.LogError("Script info response too large: {ContentLength} bytes",
                        response.Content.Headers.ContentLength);
                    return;
                }

                var scriptInfoJson = await response.Content.ReadAsStringAsync();
                ScriptInfo = JsonSerializer.Deserialize<ScriptInfo>(scriptInfoJson, MessageConfig.JsonOptions) ??
                             new ScriptInfo();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to fetch script info from server.");
            }
        });
    }
}