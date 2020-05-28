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
    /// Enumeration of state update types.
    /// </summary>
    public enum StateUpdateType
    {
        /// <summary>
        /// Add a component to the database.
        /// </summary>
        Add,

        /// <summary>
        /// Modify an existing component in the database.
        /// </summary>
        Modify,

        /// <summary>
        /// Remove an existing component from the database.
        /// </summary>
        Remove
    }

}
