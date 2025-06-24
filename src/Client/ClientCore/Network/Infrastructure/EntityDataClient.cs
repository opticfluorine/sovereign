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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     REST client for retrieving entity key-value data.
/// </summary>
public class EntityDataClient(RestClient restClient, ILogger<EntityDataClient> logger)
{
    private const int MaxContentLength = 65536; // 64 KB
    private readonly ConcurrentDictionary<ulong, Dictionary<string, string>> entityDataCache = new();

    /// <summary>
    ///     Asynchronously refreshes the entity key-value data for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void RefreshEntity(ulong entityId)
    {
        // Invalidate cache before beginning so that "laoding..." displays will work.
        entityDataCache.TryRemove(entityId, out _);

        // Asynchronously fetch latest entity key-value data from server.
        Task.Run(async () =>
        {
            try
            {
                var response = await restClient.Get($"{RestEndpoints.EntityData}/{entityId:X}");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError("Failed to refresh entity data for ID {EntityId:X}: {StatusCode}",
                        entityId, response.StatusCode);
                    return;
                }

                if (response.Content.Headers.ContentLength > MaxContentLength)
                {
                    logger.LogError("Response too long ({ContentLength} bytes) for entity ID {EntityId:X}",
                        response.Content.Headers.ContentLength, entityId);
                    return;
                }

                var data = await response.Content.ReadFromJsonAsync<EntityDataResponse>(MessageConfig.JsonOptions);
                if (data == null)
                {
                    logger.LogError("Null EntityDataResponse for entity ID {EntityId:X}", entityId);
                    return;
                }

                entityDataCache[entityId] = data.EntityData;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error refreshing entity data for ID {EntityId:X}", entityId);
            }
        });
    }

    /// <summary>
    ///     Gets the latest cached entity data if it is not currently being refreshed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="entityData">Entity key-value data. Only valid if method returns true.</param>
    /// <returns>true if a current cached copy is available, false otherwise.</returns>
    public bool TryGetCachedEntityData(ulong entityId, [NotNullWhen(true)] out Dictionary<string, string>? entityData)
    {
        return entityDataCache.TryGetValue(entityId, out entityData);
    }
}