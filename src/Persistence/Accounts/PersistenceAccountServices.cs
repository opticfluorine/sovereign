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

using Sovereign.EngineUtil.Monads;
using Sovereign.Persistence.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.Persistence.Accounts
{

    /// <summary>
    /// Public API exported by Persistence to provide account-related
    /// database services.
    /// </summary>
    public sealed class PersistenceAccountServices
    {
        private readonly PersistenceProviderManager providerManager;

        private const int FIELD_ID = 0;
        private const int FIELD_SALT = 2;
        private const int FIELD_HASH = 3;
        private const int FIELD_OPSLIMIT = 4;
        private const int FIELD_MEMLIMIT = 5;

        public PersistenceAccountServices(PersistenceProviderManager providerManager)
        {
            this.providerManager = providerManager;
        }

        /// <summary>
        /// Blocking call that retrieves the account with the given username.
        /// The returned account does not include the authentication details.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns>Maybe object that contains the retrieved account, if any.</returns>
        public Maybe<Account> RetrieveAccount(string username)
        {
            var query = providerManager.PersistenceProvider.RetrieveAccountQuery;
            using (var reader = query.RetrieveAccount(username))
            {
                if (reader.Reader.Read())
                {
                    // Account found, convert.
                    var id = new Guid((byte[])reader.Reader.GetValue(FIELD_ID));
                    return new Maybe<Account>(new Account(id, username));
                }
                else
                {
                    // No account found, return empty record.
                    return new Maybe<Account>();
                }
            }
        }

        /// <summary>
        /// Blocking call that retrieves the account with the given username,
        /// including its associated authentication details.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns>Maybe object that contains the retrieve account, if any.</returns>
        public Maybe<Account> RetrieveAccountWithAuth(string username)
        {
            var query = providerManager.PersistenceProvider.RetrieveAccountWithAuthQuery;
            using (var reader = query.RetrieveAccountWithAuth(username))
            {
                if (reader.Reader.Read())
                {
                    // Account found, convert.
                    var id = new Guid((byte[])reader.Reader.GetValue(FIELD_ID));
                    var salt = (byte[])reader.Reader.GetValue(FIELD_SALT);
                    var hash = (byte[])reader.Reader.GetValue(FIELD_HASH);
                    var opslimit = (ulong)reader.Reader.GetValue(FIELD_OPSLIMIT);
                    var memlimit = (ulong)reader.Reader.GetValue(FIELD_MEMLIMIT);
                    return new Maybe<Account>(new Account(id, username, salt, hash,
                        opslimit, memlimit));
                }
                else
                {
                    // No account found, return empty record.
                    return new Maybe<Account>();
                }
            }
        }

    }

}
