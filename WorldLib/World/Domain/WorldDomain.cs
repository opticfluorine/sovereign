using MessagePack;
using Sovereign.WorldLib.World.Domain.Block;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain
{

    /// <summary>
    /// Describes a single world domain.
    /// </summary>
    [MessagePackObject]
    public class WorldDomain
    {

        /// <summary>
        /// Domain ID. Unique within a World.
        /// </summary>
        [Key(0)]
        public uint DomainId { get; set; }

        /// <summary>
        /// Domain blocks within this domain.
        /// </summary>
        [Key(1)]
        public IList<DomainBlock> DomainBlocks { get; set; }

    }

}
