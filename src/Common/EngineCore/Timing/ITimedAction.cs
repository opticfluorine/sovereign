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

namespace Sovereign.EngineCore.Timing;

/// <summary>
///     Describes an action that is executed in the main thread with
///     a given interval.
/// </summary>
public interface ITimedAction
{
    /// <summary>
    ///     Interval in microseconds between successive actions.
    /// </summary>
    ulong Interval { get; }

    /// <summary>
    ///     Invokes the action.
    /// </summary>
    /// <param name="triggerTime">
    ///     System time at which the action was triggered.
    ///     This is a multiple of the Interval.
    /// </param>
    void Invoke(ulong triggerTime);
}