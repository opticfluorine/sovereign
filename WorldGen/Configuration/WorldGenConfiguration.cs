/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
