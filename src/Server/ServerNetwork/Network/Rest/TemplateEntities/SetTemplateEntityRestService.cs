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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.ServerCore.Systems.TemplateEntity;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST endpoint for creating or updating template entities.
/// </summary>
public sealed class SetTemplateEntityRestService(
    EntityDefinitionValidator definitionValidator,
    IEventSender eventSender,
    ILogger<SetTemplateEntityRestService> logger,
    TemplateEntityController templateEntityController)
{
    /// <summary>
    ///     POST endpoint for setting template entity data.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="context">Context.</param>
    /// <returns>Result.</returns>
    public Task<IResult> SetTemplateEntityPost([FromBody] KeyedEntityDefinitionEventDetails request,
        HttpContext context)
    {
        try
        {
            if (!definitionValidator.Validate(request.EntityDefinition) ||
                !EntityUtil.IsTemplateEntity(request.EntityDefinition.EntityId))
            {
                logger.LogError("Invalid definition received. (Request ID: {Id}", context.TraceIdentifier);
                return Task.FromResult(Results.BadRequest());
            }

            // All keys and values are valid for entity data, so no need to validate the dictionary.
            // If we get here, the request is valid and we can process the update.
            templateEntityController.UpdateTemplateEntity(eventSender, request.EntityDefinition,
                request.EntityKeyValuePairs);
            return Task.FromResult(Results.Ok());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing SetTemplateEntity request. (Request ID: {Id})",
                context.TraceIdentifier);
            return Task.FromResult(Results.InternalServerError());
        }
    }
}