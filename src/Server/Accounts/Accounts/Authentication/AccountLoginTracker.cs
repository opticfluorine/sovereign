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

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Responsible for tracking which accounts are currently logged in.
/// </summary>
public sealed class AccountLoginTracker
{
    /// <summary>
    ///     Map from connection IDs to logged in account IDs.
    /// </summary>
    private readonly Dictionary<int, Guid> connectionsToAccounts = new();

    /// <summary>
    ///     Set of all currently logged in account IDs.
    /// </summary>
    private readonly HashSet<Guid> loggedInAccountIds = new();

    /// <summary>
    ///     Signals that the given account has logged in.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    public void Login(Guid accountId)
    {
        loggedInAccountIds.Add(accountId);
    }

    /// <summary>
    ///     Signals that the given account has logged out.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    public void Logout(Guid accountId)
    {
        loggedInAccountIds.Remove(accountId);
    }

    /// <summary>
    ///     Checks whether the given account ID is already logged in.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>true if logged in, false otherwise.</returns>
    public bool IsLoggedIn(Guid accountId)
    {
        return loggedInAccountIds.Contains(accountId);
    }

    /// <summary>
    ///     Associates a logged in account to its event server connection.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="connectionId">Event server connection ID.</param>
    /// <exception cref="InvalidOperationException">Thrown if the connection ID is already mapped.</exception>
    public void AssociateConnection(Guid accountId, int connectionId)
    {
        if (connectionsToAccounts.TryGetValue(connectionId, out var otherAcctId))
            throw new InvalidOperationException(
                $"Connection ID {connectionId} is already mapped to account {otherAcctId}.");

        connectionsToAccounts.Add(connectionId, accountId);
    }

    /// <summary>
    ///     If the given connection is associated to an account, triggers a logout for the account.
    /// </summary>
    /// <param name="connectionId">Connection ID.</param>
    public void HandleDisconnect(int connectionId)
    {
        if (!connectionsToAccounts.TryGetValue(connectionId, out var accountId)) return;
        connectionsToAccounts.Remove(connectionId);
        Logout(accountId);
    }
}