using Engine8.WorldLib.World.Domain.Block;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.WorldLib.World.Domain
{

    /// <summary>
    /// Describes a single world domain.
    /// </summary>
    public class WorldDomain
    {

        /// <summary>
        /// Domain ID. Unique within a World.
        /// </summary>
        public uint DomainId { get; set; }

        /// <summary>
        /// Domain blocks within this domain.
        /// </summary>
        public IList<DomainBlock> DomainBlocks { get; set; }

    }

}
