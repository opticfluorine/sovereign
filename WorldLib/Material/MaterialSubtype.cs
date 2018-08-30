﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.Material
{

    /// <summary>
    /// Describes a subtype of a material.
    /// </summary>
    public class MaterialSubtype
    {

        /// <summary>
        /// The material modifier value. Unique within a material.
        /// </summary>
        public uint MaterialModifier { get; set; }

        /// <summary>
        /// The ID of the tile sprite used for the top face.
        /// </summary>
        public uint TopFaceTileSpriteId { get; set; }

        /// <summary>
        /// The ID of the tile sprite used for the side face.
        /// </summary>
        public uint SideFaceTileSpriteId { get; set; }

    }

}
