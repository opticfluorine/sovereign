/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Defines delegate types for entity-related events.
/// </summary>
public static class EntityEventDelegates
{
    /// <summary>
    ///     Delegate for entity removal events.
    /// </summary>
    /// <param name="entityId">Entity ID that is removed.</param>
    public delegate void RemoveEntityEvent(ulong entityId);

    /// <summary>
    ///     Delegate for entity unload events.
    /// </summary>
    /// <param name="entityId">Entity ID that is unloaded.</param>
    public delegate void UnloadEntityEvent(ulong entityId);
}