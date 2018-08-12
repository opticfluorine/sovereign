using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Components
{

    /// <summary>
    /// Event arguments for component add and modify events.
    /// </summary>
    /// <typeparam name="T">Component value type.</typeparam>
    public class ComponentAddModifyEventArgs<T> : EventArgs
    {
        
        /// <summary>
        /// Entity ID added or modified.
        /// </summary>
        public ulong EntityId { get; private set; }

        /// <summary>
        /// Latest value.
        /// </summary>
        public T Value { get; private set; }

        public ComponentAddModifyEventArgs(ulong entityId, T value)
        {
            EntityId = entityId;
            Value = value;
        }

    }

}
