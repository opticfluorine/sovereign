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

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Persistence.State
{

    /// <summary>
    /// Record of a persistence state update.
    /// </summary>
    /// <typeparam name="T">Component value type.</typeparam>
    public struct StateUpdate<T> where T : unmanaged
    {

        /// <summary>
        /// Internal entity ID.
        /// </summary>
        public ulong EntityId;

        /// <summary>
        /// State update type.
        /// </summary>
        public StateUpdateType StateUpdateType;

        /// <summary>
        /// Latest value.
        /// </summary>
        public T Value;

    }

}
