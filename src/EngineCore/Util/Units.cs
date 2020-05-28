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

namespace Sovereign.EngineCore.Util
{

    /// <summary>
    /// Constants for unit conversions.
    /// </summary>
    public static class Units
    {

        /// <summary>
        /// Unit conversion constants related to system time.
        /// </summary>
        public static class SystemTime
        {

            /// <summary>
            /// One minute in system time.
            /// </summary>
            public const ulong Minute = 60 * Second;

            /// <summary>
            /// One second in system time.
            /// </summary>
            public const ulong Second = 1000 * Millisecond;

            /// <summary>
            /// One ms in system time.
            /// </summary>
            public const ulong Millisecond = 1000 * Microsecond;

            /// <summary>
            /// One us in system time.
            /// </summary>
            public const ulong Microsecond = 1;

        }

    }

}
