/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Resources;
using System.IO;

namespace Sovereign.ClientCore.Resources
{

    /// <summary>
    /// Builds resource paths for the client.
    /// </summary>
    public class ClientResourcePathBuilder : IResourcePathBuilder
    {

        /// <summary>
        /// Top-level resource directory.
        /// </summary>
        private const string ResourceRoot = "Data";

        public string BuildPathToResource(ResourceType resourceType, string resourceFilename)
        {
            return Path.Combine(GetBaseDirectoryForResource(resourceType), resourceFilename);
        }

        public string GetBaseDirectoryForResource(ResourceType resourceType)
        {
            return Path.Combine(ResourceRoot, resourceType.ToString());
        }

    }

}
