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
using System.Text;
using Castle.Core.Logging;
using Sodium;
using Sovereign.Persistence.Accounts;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Responsible for authenticating account credentials.
/// </summary>
public sealed class AccountAuthenticator
{
    private readonly PersistenceAccountServices persistenceAccountServices;

    public AccountAuthenticator(PersistenceAccountServices persistenceAccountServices)
    {
        this.persistenceAccountServices = persistenceAccountServices;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Attempts to authenticate the given credentials.
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
    ///     Checks authentication of an account.
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
        return AuthenticationUtil.CompareHashes(trialHash, account.Hash);
    }
}