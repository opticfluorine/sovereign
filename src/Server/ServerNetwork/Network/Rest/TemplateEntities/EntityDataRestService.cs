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
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineUtil.Collections;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST endpoint for retrieving entity data.
/// </summary>
public sealed class EntityDataRestService(
    ILogger<EntityDataRestService> logger,
    IDataServices dataServices)
{
    private readonly ObjectPool<Dictionary<string, string>> dataPool = new();

    /// <summary>
    ///     GET endpoint for retrieving entity data.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> EntityDataGet([FromRoute] ulong entityId, HttpContext context)
    {
        try
        {
            var data = dataPool.TakeObject();
            string responseJson;
            try
            {
                dataServices.GetEntityData(entityId, data);
                var response = new EntityDataResponse
                {
                    EntityData = data
                };
                responseJson = JsonSerializer.Serialize(response, MessageConfig.JsonOptions);
            }
            finally
            {
                dataPool.ReturnObject(data);
            }

            return Task.FromResult(Results.Text(responseJson, "application/json", Encoding.UTF8, 200));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing entity data request for entity ID {EntityId:X}. (Request ID: {Id})",
                entityId, context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError());
        }
    }
}