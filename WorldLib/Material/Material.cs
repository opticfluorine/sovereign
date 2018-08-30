using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.Material
{

    /// <summary>
    /// Describes a material.
    /// </summary>
    public class Material
    {

        /// <summary>
        /// Material ID. Unique.
        /// </summary>
        public uint MaterialId { get; set; }

        /// <summary>
        /// Name of the material.
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// Associated material subtypes.
        /// </summary>
        public IList<MaterialSubtype> MaterialSubtypes { get; set; }

    }

}
