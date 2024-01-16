/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Base class for component indexers.
/// </summary>
/// <typeparam name="T">Component value type.</typeparam>
public class BaseComponentIndexer<T> : IDisposable
    where T : notnull
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
        eventSource.OnEndUpdates += EndUpdatesCallback;
    }

    public void Dispose()
    {
        eventSource.OnEndUpdates -= EndUpdatesCallback;
        eventSource.OnComponentRemoved -= ComponentRemovedCallback;
        eventSource.OnComponentModified -= ComponentModifiedCallback;
        eventSource.OnComponentAdded -= ComponentAddedCallback;
        eventSource.OnStartUpdates -= StartUpdatesCallback;
    }

    /// <summary>
    ///     Called when the event source signals that updates are beginning.
    ///     Defaults to no action.
    /// </summary>
    protected virtual void StartUpdatesCallback()
    {
    }

    /// <summary>
    ///     Called when the event source signals that a component is added.
    ///     Defaults to no action.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">New component value.</param>
    /// <param name="isLoad">If true, indicates the component was loaded rather than added.</param>
    protected virtual void ComponentAddedCallback(ulong entityId, T componentValue, bool isLoad)
    {
    }

    /// <summary>
    ///     Called when the event source signals that a component is modified.
    ///     Defaults to no action.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">New component value.</param>
    protected virtual void ComponentModifiedCallback(ulong entityId, T componentValue)
    {
    }

    /// <summary>
    ///     Called when the event source signals that a component is removed.
    ///     Defaults to no action.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">If true, indicates the component was unloaded rather than removed.</param>
    protected virtual void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
    }

    /// <summary>
    ///     Called when the event source signals that updates are complete.
    ///     Defaults to no action.
    /// </summary>
    protected virtual void EndUpdatesCallback()
    {
    }
}