/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

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
