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

using System.Data;

namespace Sovereign.Persistence.Database.Queries
{

    /// <summary>
    /// Generic query interface for removing components from the database.
    /// </summary>
    public interface IRemoveComponentQuery
    {

        /// <summary>
        /// Removes the component for the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="transaction">Database transaction.</param>
        void Remove(ulong entityId, IDbTransaction transaction);

    }

}
