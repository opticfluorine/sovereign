using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain.Block
{

    /// <summary>
    /// Describes a single block within a domain.
    /// </summary>
    [MessagePackObject]
    public class DomainBlock
    {

        /// <summary>
        /// X coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(0)]
        public int X { get; set; }

        /// <summary>
        /// Y coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(1)]
        public int Y { get; set; }

        /// <summary>
        /// Z coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(2)]
        public int Z { get; set; }

        /// <summary>
        /// Material ID of this block.
        /// </summary>
        [Key(3)]
        public uint MaterialId { get; set; }

        /// <summary>
        /// Material modifier of this block.
        /// </summary>
        [Key(4)]
        public uint MaterialModifier { get; set; }

    }
}
