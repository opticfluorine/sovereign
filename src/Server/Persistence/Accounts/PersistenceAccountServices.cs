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

using System;
using Sovereign.EngineUtil.Monads;
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Accounts;

/// <summary>
///     Public API exported by Persistence to provide account-related
///     database services.
/// </summary>
public sealed class PersistenceAccountServices
{
    private const int FIELD_ID = 0;
    private const int FIELD_SALT = 1;
    private const int FIELD_HASH = 2;
    private const int FIELD_OPSLIMIT = 3;
    private const int FIELD_MEMLIMIT = 4;
    private readonly PersistenceProviderManager providerManager;

    public PersistenceAccountServices(PersistenceProviderManager providerManager)
    {
        this.providerManager = providerManager;
    }

    /// <summary>
    ///     Blocking call that retrieves the account with the given username.
    ///     The returned account does not include the authentication details.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <returns>Maybe object that contains the retrieved account, if any.</returns>
    public Maybe<Account> RetrieveAccount(string username)
    {
        var query = providerManager.PersistenceProvider.RetrieveAccountQuery;
        using (var reader = query!.RetrieveAccount(username))
        {
            if (reader.Reader.Read())
            {
                // Account found, convert.
                var id = new Guid((byte[])reader.Reader.GetValue(FIELD_ID));
                return new Maybe<Account>(new Account(id, username));
            }

            // No account found, return empty record.
            return new Maybe<Account>();
        }
    }

    /// <summary>
    ///     Blocking call that retrieves the account with the given username,
    ///     including its associated authentication details.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <returns>Maybe object that contains the retrieve account, if any.</returns>
    public Maybe<Account> RetrieveAccountWithAuth(string username)
    {
        var query = providerManager.PersistenceProvider.RetrieveAccountWithAuthQuery;
        using (var reader = query!.RetrieveAccountWithAuth(username))
        {
            if (reader.Reader.Read())
            {
                // Account found, convert.
                var id = new Guid((byte[])reader.Reader.GetValue(FIELD_ID));
                var salt = (byte[])reader.Reader.GetValue(FIELD_SALT);
                var hash = (byte[])reader.Reader.GetValue(FIELD_HASH);
                var opslimit = (long)reader.Reader.GetValue(FIELD_OPSLIMIT);
                var memlimit = (long)reader.Reader.GetValue(FIELD_MEMLIMIT);
                return new Maybe<Account>(new Account(id, username, salt, hash,
                    (ulong)opslimit, (ulong)memlimit));
            }

            // No account found, return empty record.
            return new Maybe<Account>();
        }
    }
}