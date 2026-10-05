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
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sovereign.ServerCore.Systems.WorldGeneration;

namespace Sovereign.ServerNetwork.Network.Rest.WorldGen;

/// <summary>
///     REST endpoint for saving world generation profiles.
/// </summary>
public sealed class SetWorldGenProfileRestService(
    WorldGenProfileStore profileStore,
    ILogger<SetWorldGenProfileRestService> logger)
{
    /// <summary>
    ///     PUT endpoint for saving a world generation profile under the given name. The
    ///     request body is a JSON-serialized <c>WorldGenProfile</c>.
    /// </summary>
    /// <param name="name">Profile name, without extension.</param>
    /// <param name="body">JSON-serialized world generation profile.</param>
    /// <param name="context">HTTP context.</param>
    /// <returns>Result.</returns>
    public IResult PutProfile(string name, [FromBody] JsonElement body, HttpContext context)
    {
        try
        {
            if (!profileStore.TrySaveProfile(name, body.GetRawText(), out var error))
            {
                logger.LogInformation(
                    "Rejected world generation profile PUT for \"{Name}\": {Error} (Request ID: {Id})",
                    name, error, context.TraceIdentifier);
                return Results.BadRequest(error);
            }

            return Results.NoContent();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing SetWorldGenProfile request. (Request ID: {Id})",
                context.TraceIdentifier);
            return Results.InternalServerError();
        }
    }
}
