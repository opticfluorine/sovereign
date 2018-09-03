using Sovereign.EngineCore.World.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.World
{

    /// <summary>
    /// Responsible for managing all world-related resources.
    /// </summary>
    public class WorldManager
    {

        /// <summary>
        /// Material manager.
        /// </summary>
        private readonly MaterialManager materialManager;

        public WorldManager(MaterialManager materialManager)
        {
            this.materialManager = materialManager;
        }

    }

}
