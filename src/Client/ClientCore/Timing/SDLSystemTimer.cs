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
using SDL2;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Timing;

/// <summary>
///     SFML-backed implementation of ISystemTimer.
/// </summary>
public class SDLSystemTimer : ISystemTimer
{
    /// <summary>
    ///     Initial value of the counter.
    /// </summary>
    private readonly ulong baseCount;

    /// <summary>
    ///     High resolution counter frequency in counts per second.
    /// </summary>
    private readonly ulong performanceFrequency;

    public SDLSystemTimer()
    {
        performanceFrequency = SDL.SDL_GetPerformanceFrequency() / 1000000;
        baseCount = SDL.SDL_GetPerformanceCounter();

        if (performanceFrequency == 0)
            throw new ApplicationException("System timer has insufficient precision.");
    }

    /// <summary>
    ///     Gets the current system time in microseconds.
    /// </summary>
    /// <returns>Current system time in us.</returns>
    public ulong GetTime()
    {
        return (SDL.SDL_GetPerformanceCounter() - baseCount) / performanceFrequency;
    }
}