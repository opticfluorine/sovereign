using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.WorldGen.Configuration
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
