/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Interface for retrieving an account from the database.
/// </summary>
public interface IRetrieveAccountQuery
{
    /// <summary>
    ///     Retrieves the account with the given username, if any.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <returns>QueryReader to supply the result.</returns>
    QueryReader RetrieveAccount(string username);
}