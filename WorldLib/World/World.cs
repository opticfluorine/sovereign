using Engine8.WorldLib.World.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.WorldLib.World
{

    /// <summary>
    /// Describes a game world.
    /// </summary>
    public class World
    {

        /// <summary>
        /// The list of world domains that comprise the game world.
        /// </summary>
        public IList<WorldDomain> WorldDomains { get; set; }

    }

}
