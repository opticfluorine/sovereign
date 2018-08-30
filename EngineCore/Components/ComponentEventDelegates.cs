using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Defines delegate types for component add/remove/modify events.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public static class ComponentEventDelegates<T>
    {

        /// <summary>
        /// Delegate type used to communicate component add and update events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">New component value.</param>
        public delegate void ComponentEventHandler(ulong entityId, T componentValue);

        /// <summary>
        /// Delegate type used to communicate component remove events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public delegate void ComponentRemovedEventHandler(ulong entityId);

    }

}
