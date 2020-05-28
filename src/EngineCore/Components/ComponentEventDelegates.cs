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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Defines delegate types for component add/remove/modify events.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public static class ComponentEventDelegates<T>
    {

        /// <summary>
        /// Delegate type used to communicate component add and update events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">New component value.</param>
        public delegate void ComponentEventHandler(ulong entityId, T componentValue);

        /// <summary>
        /// Delegate type used to communicate component remove events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public delegate void ComponentRemovedEventHandler(ulong entityId);

        /// <summary>
        /// Delegate type used to communicate component unload events.
        /// </summary>
        /// <param name="entityId"></param>
        public delegate void ComponentUnloadedEventHandler(ulong entityId);

    }

}
