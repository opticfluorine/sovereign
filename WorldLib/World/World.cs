using MessagePack;
using Sovereign.WorldLib.World.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World
{

    /// <summary>
    /// Describes a game world.
    /// </summary>
    [MessagePackObject]
    public class World
    {

        /// <summary>
        /// The list of world domains that comprise the game world.
        /// </summary>
        [Key(0)]
        public IList<WorldDomain> WorldDomains { get; set; }

    }

}
