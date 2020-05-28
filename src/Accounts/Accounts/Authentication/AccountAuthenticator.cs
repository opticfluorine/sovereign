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

using Castle.Core.Logging;
using Sodium;
using Sovereign.EngineUtil.Monads;
using Sovereign.Persistence.Accounts;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sovereign.Accounts.Accounts.Authentication
{

    /// <summary>
    /// Responsible for authenticating account credentials.
    /// </summary>
    public sealed class AccountAuthenticator
    {
        private readonly PersistenceAccountServices persistenceAccountServices;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public AccountAuthenticator(PersistenceAccountServices persistenceAccountServices)
        {
            this.persistenceAccountServices = persistenceAccountServices;
        }

        /// <summary>
        /// Attempts to authenticate the given credentials.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="id">Account ID; only valid if the method returns true.</param>
        /// <returns>true if authentication succeeded, false otherwise.</returns>
        public bool Authenticate(string username, string password, out Guid id)
        {
            try
            {
                // Retrieve account details from database.
                var account = persistenceAccountServices.RetrieveAccountWithAuth(username);
                if (!account.HasValue)
                {
                    // Account does not exist.
                    Logger.Debug("Username \"" + username + "\" not known; rejecting.");
                    id = Guid.Empty;
                    return false;
                }

                // Check trial hash and return.
                id = account.Value.Id;
                return CheckAccount(account.Value, password);
            }
            catch (Exception e)
            {
                Logger.Error("Error while authenticating account with username \""
                    + username + "\"; rejecting.", e);
                id = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// Checks authentication of an account.
        /// </summary>
        /// <param name="account">Account.</param>
        /// <param name="password">Password for attempted login.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool CheckAccount(Account account, string password)
        {
            // Compute trial hash.
            var pwdBytes = Encoding.UTF8.GetBytes(password);
            var trialHash = PasswordHash.ArgonHashBinary(pwdBytes,
                account.Salt,
                (long)account.Opslimit,
                (int)account.Memlimit);

            // Sanity check on hash sizes.
            if (trialHash.Length != account.Hash.Length)
            {
                // Hash sizes differ.
                Logger.ErrorFormat("Hash sizes for user {0} differ ({1} vs {2}); rejecting.",
                    account.Username, trialHash.Length, account.Hash.Length);
                return false;
            }

            // Hash comparison.
            return CompareHashes(trialHash, account.Hash);
        }

        /// <summary>
        /// Securely compares two hashes in a way that is resistant to timing
        /// attacks.
        /// </summary>
        /// <param name="left">First hash.</param>
        /// <param name="right">Second hash.</param>
        /// <returns>true if equal, false otherwise.</returns>
        /// <remarks>
        /// Optimizations are disabled for this method to prevent the compiler
        /// and JITter from re-introducing vulnerability through loop
        /// optimization.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private bool CompareHashes(byte[] left, byte[] right)
        {
            // PRE: Hash lengths are equal.

            // Loop over the entire array, performing a constant time check.
            var check = 0;
            for (int i = 0; i < left.Length; ++i)
            {
                check |= (left[i] ^ right[i]);
            }
            return check == 0;
        }

    }

}
