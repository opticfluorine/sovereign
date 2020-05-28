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

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldGen.Configuration
{

    /// <summary>
    /// Validates WorldGenConfiguration objects.
    /// </summary>
    public class WorldGenConfigurationValidator
    {

        /// <summary>
        /// Determines whether a WorldGen configuration is valid.
        /// </summary>
        /// <param name="worldGenConfiguration">WorldGen configuration.</param>
        /// <returns>true if the configuration is valid, false otherwise.</returns>
        public bool IsValid(WorldGenConfiguration worldGenConfiguration)
        {
            return true;
        }

    }

}
