/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
