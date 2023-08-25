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