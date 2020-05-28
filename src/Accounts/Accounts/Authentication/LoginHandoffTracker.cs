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
