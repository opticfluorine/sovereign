using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Components
{

    /// <summary>
    /// Event arguments for component remove events.
    /// </summary>
    public class ComponentRemoveEventArgs : EventArgs
    {

        /// <summary>
        /// Entity ID that was removed.
        /// </summary>
        public ulong EntityId { get; private set; }

        public ComponentRemoveEventArgs(ulong entityId)
        {
            EntityId = entityId;
        }

    }
}
