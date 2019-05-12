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

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Base class for component indexers.
    /// </summary>
    /// <typeparam name="T">Component value type.</typeparam>
    public class BaseComponentIndexer<T> : IDisposable
    {
        protected readonly BaseComponentCollection<T> components;
        protected readonly IComponentEventSource<T> eventSource;

        public BaseComponentIndexer(BaseComponentCollection<T> components,
            IComponentEventSource<T> eventSource)
        {
            this.components = components;
            this.eventSource = eventSource;

            eventSource.OnStartUpdates += StartUpdatesCallback;
            eventSource.OnComponentAdded += ComponentAddedCallback;
            eventSource.OnComponentModified += ComponentModifiedCallback;
            eventSource.OnComponentRemoved += ComponentRemovedCallback;
            eventSource.OnComponentUnloaded += ComponentUnloadedCallback;
            eventSource.OnEndUpdates += EndUpdatesCallback;
        }

        /// <summary>
        /// Called when the event source signals that updates are beginning.
        /// Defaults to no action.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void StartUpdatesCallback(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when the event source signals that a component is added.
        /// Defaults to no action.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">New component value.</param>
        protected virtual void ComponentAddedCallback(ulong entityId, T componentValue)
        {
        }

        /// <summary>
        /// Called when the event source signals that a component is modified.
        /// Defaults to no action.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">New component value.</param>
        protected virtual void ComponentModifiedCallback(ulong entityId, T componentValue)
        {
        }

        /// <summary>
        /// Called when the event source signals that a component is removed.
        /// Defaults to no action.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        protected virtual void ComponentRemovedCallback(ulong entityId)
        {
        }

        /// <summary>
        /// Called when the event source signals that a component is unloaded.
        /// Defaults to no action.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        protected virtual void ComponentUnloadedCallback(ulong entityId)
        {
        }

        /// <summary>
        /// Called when the event source signals that updates are complete.
        /// Defaults to no action.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void EndUpdatesCallback(object sender, EventArgs e)
        {
        }

        public void Dispose()
        {
            eventSource.OnEndUpdates -= EndUpdatesCallback;
            eventSource.OnComponentUnloaded -= ComponentUnloadedCallback;
            eventSource.OnComponentRemoved -= ComponentRemovedCallback;
            eventSource.OnComponentModified -= ComponentModifiedCallback;
            eventSource.OnComponentAdded -= ComponentAddedCallback;
            eventSource.OnStartUpdates -= StartUpdatesCallback;
        }

    }

}
