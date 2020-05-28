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
        public event ComponentEventDelegates<T>.ComponentUnloadedEventHandler OnComponentUnloaded;
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

        protected override void ComponentUnloadedCallback(ulong entityId)
        {
            if (ShouldAccept(entityId)) OnComponentUnloaded(entityId);
        }

    }

}
