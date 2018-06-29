/*
 * Engine8 Dynamic World MMORPG Engine
 * Copyright (c) 2018 opticfluorine
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Engine8.EngineCore.Timing;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Timing
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
