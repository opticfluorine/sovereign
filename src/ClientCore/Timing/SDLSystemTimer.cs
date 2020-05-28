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

using Sovereign.EngineCore.Timing;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Timing
{

    /// <summary>
    /// SFML-backed implementation of ISystemTimer.
    /// </summary>
    public class SDLSystemTimer : ISystemTimer
    {

        /// <summary>
        /// High resolution counter frequency in counts per second.
        /// </summary>
        private ulong performanceFrequency;

        /// <summary>
        /// Initial value of the counter.
        /// </summary>
        private ulong baseCount;

        public SDLSystemTimer()
        {
            performanceFrequency = SDL.SDL_GetPerformanceFrequency();
            baseCount = SDL.SDL_GetPerformanceCounter();
        }

        /// <summary>
        /// Gets the current system time in microseconds.
        /// </summary>
        /// <returns>Current system time in us.</returns>
        public ulong GetTime()
        {
            return 1000000 * (SDL.SDL_GetPerformanceCounter() - baseCount) 
                / performanceFrequency;
        }

    }

}
