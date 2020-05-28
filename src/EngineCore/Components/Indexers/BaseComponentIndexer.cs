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
