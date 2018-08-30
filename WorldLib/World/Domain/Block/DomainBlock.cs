using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain.Block
{

    /// <summary>
    /// Describes a single block within a domain.
    /// </summary>
    public class DomainBlock
    {

        /// <summary>
        /// X coordinate of the top-northwest corner of the block.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate of the top-northwest corner of the block.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Z coordinate of the top-northwest corner of the block.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Material ID of this block.
        /// </summary>
        public uint MaterialId { get; set; }

        /// <summary>
        /// Material modifier of this block.
        /// </summary>
        public uint MaterialModifier { get; set; }

    }
}
