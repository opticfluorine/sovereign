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

namespace Sovereign.Persistence.Database.Queries
{

    /// <summary>
    /// Query for adding a new account.
    /// </summary>
    public interface IAddAccountQuery
    {

        /// <summary>
        /// Adds a new account to the database.
        /// </summary>
        /// <param name="id">Account ID.</param>
        /// <param name="username">Username.</param>
        /// <param name="passwordSalt">Password salt.</param>
        /// <param name="passwordHash">Password hash.</param>
        /// <param name="opslimit">Password hashing opslimit value.</param>
        /// <param name="memlimit">Password hashing memlimit value.</param>
        /// <returns>true if successful, false otherwise.</returns>
        bool AddAccount(Guid id, string username, byte[] passwordSalt, byte[] passwordHash,
            ulong opslimit, ulong memlimit);

    }

}
