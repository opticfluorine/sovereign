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

namespace Sovereign.EngineCore.Configuration;

/// <summary>
///     Engine configuration.
/// </summary>
public interface IEngineConfiguration
{
    /// <summary>
    ///     Interval between event ticks in us.
    /// </summary>
    ulong EventTickInterval { get; }

    /// <summary>
    ///     Briefly sleep the executor threads after every this many iterations.
    ///     Never sleep if set to zero.
    /// </summary>
    /// <remarks>
    ///     This trades power consumption for performance. When set to zero, each executor
    ///     thread will consume close to 100% of a CPU core. Positive values will occasionally
    ///     pause the thread, reducing CPU usage but increasing average event latency (and possibly
    ///     causing secondary negative performance effects from the CPU cache due to additional
    ///     context switching during the pauses). A value of 1 minimizes CPU consumption; larger values
    ///     progressively favor performance over CPU consumption.
    /// </remarks>
    int ExecutorThreadSleepInterval { get; }
}