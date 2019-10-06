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
    /// Tracks connection handoffs after successful login.
    /// </summary>
    public sealed class LoginHandoffTracker
    {

        /// <summary>
        /// 
        /// </summary>
        private sealed class HandoffToken
        {

            /// <summary>
            /// Account ID.
            /// </summary>
            private readonly Guid accountId;

            /// <summary>
            /// IP address from which the login request originated.
            /// </summary>
            private readonly string ipAddress;

            public HandoffToken(Guid accountId, string ipAddress)
            {
                this.accountId = accountId;
                this.ipAddress = ipAddress;
            }

            public override bool Equals(object o)
            {
                return o is HandoffToken token && this == token;
            }

            public static bool operator ==(HandoffToken left, HandoffToken right)
            {
                return (left == null && right == null) ||
                        left != null && right != null &&
                        left.accountId == right.accountId &&
                        left.ipAddress.Equals(right.ipAddress);
            }

            public static bool operator !=(HandoffToken left, HandoffToken right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                var hash = 17;
                hash = hash * 31 + accountId.GetHashCode();
                hash = hash * 31 + ipAddress.GetHashCode();
                return hash;
            }

        }

        /// <summary>
        /// Account IDs with pending handoffs.
        /// </summary>
        private readonly ISet<HandoffToken> pendingHandoffs
            = new HashSet<HandoffToken>();

        /// <summary>
        /// Adds an account to the pending handoff list.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        /// <param name="ipAddress">IP address from which the login request originated.</param>
        /// <remarks>
        /// If the account is already in the pending handoff list, this
        /// method has no effect.
        /// </remarks>
        public void AddPendingHandoff(Guid accountId, string ipAddress)
        {
            pendingHandoffs.Add(new HandoffToken(accountId, ipAddress));
        }

        /// <summary>
        /// Removes an account from the pending handoff list if it is
        /// present.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        /// <param name="ipAddress">IP address from which the connection originated.</param>
        /// <returns>true if the account was in the list; false otherwise.</returns>
        public bool TakePendingHandoff(Guid accountId, string ipAddress)
        {
            return pendingHandoffs.Remove(new HandoffToken(accountId, ipAddress));
        }

    }

}
