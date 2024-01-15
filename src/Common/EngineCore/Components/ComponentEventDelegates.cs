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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Defines delegate types for component add/remove/modify events.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public static class ComponentEventDelegates<T>
{
    /// <summary>
    ///     Delegate type used to communicate component add and update events.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">New component value.</param>
    /// <param name="isLoad">Whether this action is a load (true), or a new change (false).</param>
    public delegate void ComponentEventHandler(ulong entityId, T componentValue, bool isLoad);

    /// <summary>
    ///     Delegate type used to communicate component remove events.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Whether this action is an unload (true), or a full deletion (false).</param>
    public delegate void ComponentRemovedEventHandler(ulong entityId, bool isUnload);
}