using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Interface implemented by classes that produce component add/remove/modify events.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IComponentEventSource<T>
    {

        /// <summary>
        /// Event triggered when component updates are started.
        /// </summary>
        /// 
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        event EventHandler OnStartUpdates;

        /// <summary>
        /// Event triggered when a component is added to the collection.
        /// </summary>
        ///
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        event ComponentEventDelegates<T>.ComponentEventHandler OnComponentAdded;

        /// <summary>
        /// Event triggered when a component is removed from the collection.
        /// </summary>
        ///
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        event ComponentEventDelegates<T>.ComponentRemovedEventHandler OnComponentRemoved;

        /// <summary>
        /// Event triggered when an existing component is updated.
        /// </summary>
        /// 
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        event ComponentEventDelegates<T>.ComponentEventHandler OnComponentModified;

        /// <summary>
        /// Event triggered when component updates are complete.
        /// </summary>
        /// 
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        event EventHandler OnEndUpdates;

    }

}
