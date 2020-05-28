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

namespace Sovereign.Accounts.Accounts.Authentication
{

    /// <summary>
    /// Responsible for tracking which accounts are currently logged in.
    /// </summary>
    public sealed class AccountLoginTracker
    {

        /// <summary>
        /// Set of all currently logged in account IDs.
        /// </summary>
        private readonly ISet<Guid> LoggedInAccountIds
            = new HashSet<Guid>();

        /// <summary>
        /// Signals that the given account has logged in.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        public void Login(Guid accountId)
        {
            LoggedInAccountIds.Add(accountId);
        }

        /// <summary>
        /// Signals that the given account has logged out.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        public void Logout(Guid accountId)
        {
            LoggedInAccountIds.Remove(accountId);
        }

        /// <summary>
        /// Checks whether the given account ID is already logged in.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        /// <returns>true if logged in, false otherwise.</returns>
        public bool IsLoggedIn(Guid accountId)
        {
            return LoggedInAccountIds.Contains(accountId);
        }

    }

}
