/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
