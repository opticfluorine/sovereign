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
    /// Configuration class for world generation.
    /// </summary>
    public class WorldGenConfiguration
    {

        #region Common

        /// <summary>
        /// Random number generator seed.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Width (along x) in blocks.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Height (along y) in blocks.
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// Type of domain to generate.
        /// </summary>
        public DomainType DomainType { get; set; }

        #endregion Common

        #region Terrain

        /// <summary>
        /// Material ID used for water.
        /// </summary>
        public uint WaterMaterialId { get; set; }

        #endregion Terrain

        #region Continents

        /// <summary>
        /// Configuration for continent generation.
        /// </summary>
        ///
        /// Only valid if DomainType is set to Continents.
        public ContinentsConfiguration Continents { get; set; }

        /// <summary>
        /// Inner configuration class used for continent generation.
        /// </summary>
        public class ContinentsConfiguration
        {

            /// <summary>
            /// Number of continents to generate.
            /// </summary>
            public uint Count { get; set; }

        }

        #endregion Continents

    }

}
