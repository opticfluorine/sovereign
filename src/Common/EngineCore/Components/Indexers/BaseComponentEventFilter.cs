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
///     Base class for component event filters.
/// </summary>
public abstract class BaseComponentEventFilter<T> : BaseComponentIndexer<T>, IComponentEventSource<T>
    where T : notnull
{
    public BaseComponentEventFilter(BaseComponentCollection<T> components,
        IComponentEventSource<T> eventSource) : base(components, eventSource)
    {
    }

    public event Action? OnStartUpdates;
    public event ComponentEventDelegates<T>.ComponentAddedEventHandler? OnComponentAdded;
    public event ComponentEventDelegates<T>.ComponentRemovedEventHandler? OnComponentRemoved;
    public event ComponentEventDelegates<T>.ComponentModifiedEventHandler? OnComponentModified;
    public event Action? OnEndUpdates;

    /// <summary>
    ///     Determines whether the given entity should pass this filter.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if the entity should pass, false otherwise.</returns>
    protected abstract bool ShouldAccept(ulong entityId);

    protected override void StartUpdatesCallback()
    {
        OnStartUpdates?.Invoke();
    }

    protected override void EndUpdatesCallback()
    {
        OnEndUpdates?.Invoke();
    }

    protected override void ComponentAddedCallback(ulong entityId, T componentValue, bool isLoad)
    {
        if (ShouldAccept(entityId)) OnComponentAdded?.Invoke(entityId, componentValue, isLoad);
    }

    protected override void ComponentModifiedCallback(ulong entityId, T componentValue)
    {
        if (ShouldAccept(entityId)) OnComponentModified?.Invoke(entityId, componentValue);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        if (ShouldAccept(entityId)) OnComponentRemoved?.Invoke(entityId, isUnload);
    }
}