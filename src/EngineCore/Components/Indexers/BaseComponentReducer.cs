/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using Sovereign.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Reduces all component events in a single tick into a primary defining
    /// event at the end of each tick.
    /// </summary>
    /// <typeparam name="T">Component value type.</typeparam>
    public abstract class BaseComponentReducer<T> : BaseComponentIndexer<T> where T : unmanaged
    {
        private readonly EventId primaryEventId;
        private readonly IEventSender eventSender;

        /// <summary>
        /// Entity IDs that need an update.
        /// </summary>
        private readonly ISet<ulong> entityIds = new HashSet<ulong>();

        public BaseComponentReducer(BaseComponentCollection<T> components,
            IComponentEventSource<T> eventSource,
            EventId primaryEventId,
            IEventSender eventSender)
            : base(components, eventSource)
        {
            this.primaryEventId = primaryEventId;
            this.eventSender = eventSender;
        }

        protected override void ComponentAddedCallback(ulong entityId, T componentValue)
        {
            entityIds.Add(entityId);
        }

        protected override void ComponentModifiedCallback(ulong entityId, T componentValue)
        {
            entityIds.Add(entityId);
        }

        protected override void EndUpdatesCallback(object sender, EventArgs e)
        {
            /* Produce an event for each affected entity. */
            foreach (var entityId in entityIds)
            {
                var currentValue = components.GetComponentForEntity(entityId).Value;
                var ev = new Event(primaryEventId, CreateDetails(entityId, currentValue));
                eventSender.SendEvent(ev);
            }

            /* Clear the entity set to avoid re-sending old updates. */
            entityIds.Clear();
        }

        /// <summary>
        /// Creates event details for the primary defining event.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="value">New component value.</param>
        /// <returns>Event details.</returns>
        abstract protected IEventDetails CreateDetails(ulong entityId, T value);

    }

}
