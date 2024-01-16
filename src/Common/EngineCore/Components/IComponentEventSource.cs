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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Interface implemented by classes that produce component add/remove/modify events.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface IComponentEventSource<T>
{
    /// <summary>
    ///     Event triggered when component updates are started.
    /// </summary>
    /// This is intended for use with data view objects that are updated
    /// on the main thread once component updates for a given tick are complete.
    event Action? OnStartUpdates;

    /// <summary>
    ///     Event triggered when a component is added to the collection.
    /// </summary>
    /// This is intended for use with data view objects that are updated
    /// on the main thread once component updates for a given tick are complete.
    event ComponentEventDelegates<T>.ComponentAddedEventHandler? OnComponentAdded;

    /// <summary>
    ///     Event triggered when an existing component is updated.
    /// </summary>
    /// This is intended for use with data view objects that are updated
    /// on the main thread once component updates for a given tick are complete.
    event ComponentEventDelegates<T>.ComponentModifiedEventHandler? OnComponentModified;

    /// <summary>
    ///     Event triggered when a component is removed from the collection.
    /// </summary>
    /// This is intended for use with data view objects that are updated
    /// on the main thread once component updates for a given tick are complete.
    event ComponentEventDelegates<T>.ComponentRemovedEventHandler? OnComponentRemoved;

    /// <summary>
    ///     Event triggered when component updates are complete.
    /// </summary>
    /// This is intended for use with data view objects that are updated
    /// on the main thread once component updates for a given tick are complete.
    event Action? OnEndUpdates;
}