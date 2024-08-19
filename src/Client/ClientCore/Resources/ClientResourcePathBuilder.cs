/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Resources;

/// <summary>
///     Builds resource paths for the client.
/// </summary>
public class ClientResourcePathBuilder : IResourcePathBuilder
{
    /// <summary>
    ///     Top-level resource directory.
    /// </summary>
    private const string ResourceRoot = "Data";

    public string BuildPathToResource(ResourceType resourceType, string resourceFilename)
    {
        var path = Path.Combine(GetBaseDirectoryForResource(resourceType), resourceFilename);
        if (Path.GetRelativePath(ResourceRoot, path).StartsWith('.'))
            throw new ArgumentException("File is not in resource directory.", "resourceFilename");

        return path;
    }

    public string GetBaseDirectoryForResource(ResourceType resourceType)
    {
        return Path.Combine(ResourceRoot, resourceType.ToString());
    }
}