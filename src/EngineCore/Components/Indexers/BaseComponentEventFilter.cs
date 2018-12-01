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

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Base class for component event filters.
    /// </summary>
    public abstract class BaseComponentEventFilter<T> : BaseComponentIndexer<T>, IComponentEventSource<T>
    {
        public event EventHandler OnStartUpdates;
        public event ComponentEventDelegates<T>.ComponentEventHandler OnComponentAdded;
        public event ComponentEventDelegates<T>.ComponentRemovedEventHandler OnComponentRemoved;
        public event ComponentEventDelegates<T>.ComponentEventHandler OnComponentModified;
        public event EventHandler OnEndUpdates;

        public BaseComponentEventFilter(BaseComponentCollection<T> components,
            IComponentEventSource<T> eventSource) : base(components, eventSource)
        {
        }

        /// <summary>
        /// Determines whether the given entity should pass this filter.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>true if the entity should pass, false otherwise.</returns>
        protected abstract bool ShouldAccept(ulong entityId);

        protected override void StartUpdatesCallback(object sender, EventArgs e) 
            => OnStartUpdates(sender, e);

        protected override void EndUpdatesCallback(object sender, EventArgs e)
            => OnEndUpdates(sender, e);

        protected override void ComponentAddedCallback(ulong entityId, T componentValue)
        {
            if (ShouldAccept(entityId)) OnComponentAdded(entityId, componentValue);
        }

        protected override void ComponentModifiedCallback(ulong entityId, T componentValue)
        {
            if (ShouldAccept(entityId)) OnComponentModified(entityId, componentValue);
        }

        protected override void ComponentRemovedCallback(ulong entityId)
        {
            if (ShouldAccept(entityId)) OnComponentRemoved(entityId);
        }

    }

}
