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
    ///     Map from account ID to connection ID.
    /// </summary>
    private readonly Dictionary<Guid, int> accountsToConnections = new();

    /// <summary>
    ///     Map from connection IDs to logged in account IDs.
    /// </summary>
    private readonly Dictionary<int, Guid> connectionsToAccounts = new();

    /// <summary>
    ///     Map from player entity ID to connection ID.
    /// </summary>
    private readonly Dictionary<ulong, int> playersToConnections = new();

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

        // Select the player and update records.
        try
        {
            accountIdsToPlayerEntityIds[accountId] = playerEntityId;
            playersToConnections[playerEntityId] = accountsToConnections[accountId];
            accountLoginStates[accountId] = AccountLoginState.InGame;
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Exception while selecting player {0} for account {1}.", playerEntityId, accountId);

            // Roll back any change.
            playersToConnections.Remove(playerEntityId);
            accountIdsToPlayerEntityIds.Remove(accountId);
        }
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
    ///     Gets the connection associated with a player, if any.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="connectionId">Connection ID. Only set if return value is true.</param>
    /// <returns>true if a connection ID was found, false otherwise.</returns>
    public bool TryGetConnectionIdForPlayer(ulong playerEntityId, out int connectionId)
    {
        return playersToConnections.TryGetValue(playerEntityId, out connectionId);
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
        accountsToConnections.Add(accountId, connectionId);
    }

    /// <summary>
    ///     If the given connection is associated to an account, triggers a logout for the account.
    /// </summary>
    /// <param name="connectionId">Connection ID.</param>
    public void HandleDisconnect(int connectionId)
    {
        if (connectionsToAccounts.TryGetValue(connectionId, out var accountId))
        {
            accountsToConnections.Remove(accountId);
            connectionsToAccounts.Remove(connectionId);
            if (accountIdsToPlayerEntityIds.TryGetValue(accountId, out var entityId))
                playersToConnections.Remove(entityId);
            Logout(accountId);
        }
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