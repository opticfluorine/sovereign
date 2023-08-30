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
using Castle.Core.Logging;
using Sodium;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Responsible for tracking which accounts are currently logged in.
/// </summary>
public sealed class AccountLoginTracker
{
    /// <summary>
    ///     Size of generated API keys in bytes.
    /// </summary>
    private const int ApiKeySize = 32;

    /// <summary>
    ///     Map from account IDs to current API keys.
    /// </summary>
    private readonly Dictionary<Guid, string> accountIdsToApiKeys = new();

    /// <summary>
    ///     Map from account IDs to selected entity IDs.
    /// </summary>
    private readonly Dictionary<Guid, ulong> accountIdsToPlayerEntityIds = new();

    /// <summary>
    ///     Current login states of connected accounts.
    /// </summary>
    private readonly Dictionary<Guid, AccountLoginState> accountLoginStates = new();

    /// <summary>
    ///     Map from connection IDs to logged in account IDs.
    /// </summary>
    private readonly Dictionary<int, Guid> connectionsToAccounts = new();

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Signals that the given account has logged in.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    public void Login(Guid accountId)
    {
        // Generate temporary API key for the account.
        var apiKey = Convert.ToBase64String(SodiumCore.GetRandomBytes(ApiKeySize));
        accountIdsToApiKeys[accountId] = apiKey;

        // Advance the connection to the player selection state.
        accountLoginStates[accountId] = AccountLoginState.SelectingPlayer;
    }

    /// <summary>
    ///     Selects a player character for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerEntityId">Entity ID of player character.</param>
    public void SelectPlayer(Guid accountId, ulong playerEntityId)
    {
        // State check.
        if (GetLoginState(accountId) != AccountLoginState.SelectingPlayer)
        {
            Logger.ErrorFormat("Attempt to select player for account {0} in invalid state.", accountId);
            return;
        }

        accountIdsToPlayerEntityIds[accountId] = playerEntityId;
    }

    /// <summary>
    ///     Signals that the given account has logged out.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    public void Logout(Guid accountId)
    {
        accountIdsToApiKeys.Remove(accountId);
        accountIdsToPlayerEntityIds.Remove(accountId);
        accountLoginStates.Remove(accountId);
    }

    /// <summary>
    ///     Gets the login state of the given account ID.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Login state.</returns>
    public AccountLoginState GetLoginState(Guid accountId)
    {
        return accountLoginStates.TryGetValue(accountId, out var state) ? state : AccountLoginState.NotLoggedIn;
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

    /// <summary>
    ///     Gets the API key for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>API key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the account is not logged in.</exception>
    public string GetApiKey(Guid accountId)
    {
        return accountIdsToApiKeys[accountId];
    }
}