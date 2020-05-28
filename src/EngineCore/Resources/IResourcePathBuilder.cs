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

namespace Sovereign.EngineCore.Resources
{

    /// <summary>
    /// Builds paths to resources.
    /// </summary>
    public interface IResourcePathBuilder
    {

        /// <summary>
        /// Builds the path to the base directory for the given resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <returns>Path to the base directory for the given resource type.</returns>
        string GetBaseDirectoryForResource(ResourceType resourceType);

        /// <summary>
        /// Builds the path to the given resource file.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <param name="resourceFilename">Name of the resource file.</param>
        /// <returns>Path to the resource file.</returns>
        string BuildPathToResource(ResourceType resourceType, string resourceFilename);

    }

}
