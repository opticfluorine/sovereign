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
using Sodium;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.ServerCore.Components;

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
    ///     Account components.
    /// </summary>
    private readonly AccountComponentCollection accounts;

    /// <summary>
    ///     Map from account ID to connection ID.
    /// </summary>
    private readonly Dictionary<Guid, int> accountsToConnections = new();

    /// <summary>
    ///     Map from connection IDs to logged in account IDs.
    /// </summary>
    private readonly Dictionary<int, Guid> connectionsToAccounts = new();

    private readonly EntityManager entityManager;

    private readonly EntityHierarchyIndexer hierarchyIndexer;

    /// <summary>
    ///     Set of players that are currently in cooldown until the next persistence sync completes.
    /// </summary>
    private readonly HashSet<ulong> playersInCooldown = new();

    /// <summary>
    ///     Map from player entity ID to connection ID.
    /// </summary>
    private readonly Dictionary<ulong, int> playersToConnections = new();

    public AccountLoginTracker(AccountComponentCollection accounts, EntityHierarchyIndexer hierarchyIndexer,
        EntityManager entityManager)
    {
        this.accounts = accounts;
        this.hierarchyIndexer = hierarchyIndexer;
        this.entityManager = entityManager;
    }

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
            logger.LogError("Attempt to select player for account {0} in invalid state.", accountId);
            return;
        }

        // Select the player and update records, updating the mapping caches along the way.
        try
        {
            accountIdsToPlayerEntityIds[accountId] = playerEntityId;
            playersToConnections[playerEntityId] = accountsToConnections[accountId];
            accountLoginStates[accountId] = AccountLoginState.InGame;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while selecting player {0} for account {1}.", playerEntityId, accountId);

            // Roll back any change.
            playersToConnections.Remove(playerEntityId);
            accountIdsToPlayerEntityIds.Remove(accountId);
        }
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
        // Look for cached value.
        if (playersToConnections.TryGetValue(playerEntityId, out connectionId))
            return true;

        // If not found, try to look up the account mapping, caching the result.
        if (!accounts.HasComponentForEntity(playerEntityId)) return false;
        var accountId = accounts[playerEntityId];
        accountIdsToPlayerEntityIds[accountId] = playerEntityId;
        playersToConnections[playerEntityId] = accountsToConnections[accountId];

        connectionId = playersToConnections[playerEntityId];
        return true;
    }

    /// <summary>
    ///     Gets the player associated with a connection, if any.
    /// </summary>
    /// <param name="connectionId">Connection ID.</param>
    /// <param name="playerEntityId">Player entity ID. Only set if return value is true.</param>
    /// <returns>true if an associated player was found, false otherwise.</returns>
    public bool TryGetPlayerForConnectionId(int connectionId, out ulong playerEntityId)
    {
        if (connectionsToAccounts.TryGetValue(connectionId, out var accountId))
            return accountIdsToPlayerEntityIds.TryGetValue(accountId, out playerEntityId);

        playerEntityId = 0;
        return false;
    }

    /// <summary>
    ///     Gets the player entity ID that the given account is currently using, if any.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerEntityId">Player entity ID, if any. Only valid if the method returns true.</param>
    /// <returns>true if found, false otherwise.</returns>
    public bool TryGetPlayerForAccountId(Guid accountId, out ulong playerEntityId)
    {
        return accountIdsToPlayerEntityIds.TryGetValue(accountId, out playerEntityId);
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
            // Log the player out if already logged in.
            if (accountIdsToPlayerEntityIds.TryGetValue(accountId, out var entityId)) LogoutPlayer(entityId);

            // Log the account out.
            LogoutAccount(accountId);

            // Clean up mappings.
            accountsToConnections.Remove(accountId);
            connectionsToAccounts.Remove(connectionId);
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

    /// <summary>
    ///     Cale when a round of perisstence synchronization is complete.
    /// </summary>
    public void OnSyncComplete()
    {
        playersInCooldown.Clear();
    }

    /// <summary>
    ///     Checks whether the given player is in cooldown and cannot currently log in.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns>true if in cooldown, false otherwise.</returns>
    public bool IsPlayerInCooldown(ulong playerEntityId)
    {
        return playersInCooldown.Contains(playerEntityId);
    }

    /// <summary>
    ///     Logs out the given player.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    private void LogoutPlayer(ulong playerEntityId)
    {
        // Put the player into cooldown to prevent login before the persistence system has a chance to sync
        // the database. Otherwise a rapid logout/login cycle could "roll back" the player state to the last
        // synchronization point.
        playersInCooldown.Add(playerEntityId);

        // Unload the player entity tree.
        var entitiesToUnload = hierarchyIndexer.GetAllDescendants(playerEntityId);
        entitiesToUnload.Add(playerEntityId);
        foreach (var entityId in entitiesToUnload) entityManager.UnloadEntity(entityId);

        // Transition back to the player selection state.
        var connId = playersToConnections[playerEntityId];
        var accountId = connectionsToAccounts[connId];
        accountLoginStates[accountId] = AccountLoginState.SelectingPlayer;

        // Clean up mappings.
        playersToConnections.Remove(playerEntityId);
        accountIdsToPlayerEntityIds.Remove(accountId);
    }

    /// <summary>
    ///     Logs out the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    private void LogoutAccount(Guid accountId)
    {
        accountIdsToApiKeys.Remove(accountId);
        accountIdsToPlayerEntityIds.Remove(accountId);
        accountLoginStates.Remove(accountId);
    }

    /// <summary>
    ///     Called when a player logs out to player selection.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void Logout(ulong playerEntityId)
    {
        LogoutPlayer(playerEntityId);
    }
}