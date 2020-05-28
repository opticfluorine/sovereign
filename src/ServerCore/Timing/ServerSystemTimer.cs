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
using System;
using System.Diagnostics;

namespace Sovereign.ServerCore.Timing
{

    /// <summary>
    /// Server-side implementation of the system timer.
    /// </summary>
    public sealed class ServerSystemTimer : ISystemTimer
    {

        /// <summary>
        /// Conversion from seconds to microseconds.
        /// </summary>
        private const long SecondsToMicroseconds = 1000000;

        /// <summary>
        /// High resolution stopwatch.
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        public ServerSystemTimer()
        {
            if (!Stopwatch.IsHighResolution)
                throw new ApplicationException("No high resolution timer available.");

            stopwatch.Start();
        }

        public ulong GetTime()
        {
            return (ulong)(SecondsToMicroseconds * stopwatch.ElapsedTicks 
                / Stopwatch.Frequency);
        }

    }

}
