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

namespace Sovereign.Accounts.Accounts.Services
{

    /// <summary>
    /// Enumerated result of an authentication.
    /// </summary>
    public enum AuthenticationResult
    {

        /// <summary>
        /// The authentication was successful.
        /// </summary>
        Successful,

        /// <summary>
        /// The authentication failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The authentication was successful, but the account is already
        /// logged in.
        /// </summary>
        AlreadyLoggedIn,

        /// <summary>
        /// Too many failed attempts have been made to log into the account,
        /// and login attempts are temporarily disabled for this account.
        /// </summary>
        TooManyAttempts,

    };

}
