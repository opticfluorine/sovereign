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
using System.Diagnostics.CodeAnalysis;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.ServerNetwork.Network.Connections;

/// <summary>
///     Responsible for processing new event bus connections.
/// </summary>
public sealed class NewConnectionProcessor
{
    /// <summary>
    ///     Size of the handoff key (account ID), in bytes.
    /// </summary>
    private const int HandoffKeyBytes = 38;

    private readonly NetworkConnectionManager connectionManager;
    private readonly ILogger<NewConnectionProcessor> logger;
    private readonly LoginHandoffTracker loginHandoffTracker;
    private readonly AccountLoginTracker loginTracker;
    private readonly SharedSecretManager sharedSecretManager;

    public NewConnectionProcessor(NetworkConnectionManager connectionManager,
        LoginHandoffTracker loginHandoffTracker,
        SharedSecretManager sharedSecretManager,
        AccountLoginTracker loginTracker,
        ILogger<NewConnectionProcessor> logger)
    {
        this.connectionManager = connectionManager;
        this.loginHandoffTracker = loginHandoffTracker;
        this.sharedSecretManager = sharedSecretManager;
        this.loginTracker = loginTracker;
        this.logger = logger;
    }

    /// <summary>
    ///     Processes a new connection request.
    /// </summary>
    /// <param name="request">New connection request.</param>
    public void ProcessConnectionRequest(ConnectionRequest request)
    {
        // Get the account ID.
        var accountId = GetAccountId(request);
        if (accountId == Guid.Empty)
        {
            // Bad request.
            logger.LogInformation("Rejecting new connection from {Ip}: malformed.",
                request.RemoteEndPoint.ToString());
            request.Reject();
            return;
        }

        // Attempt handoff for the account.
        if (!AttemptHandoff(request, accountId, out var sharedSecret))
        {
            // Failed.
            logger.LogInformation("Rejecting new connection from {Ip}: bad handoff.",
                request.RemoteEndPoint.ToString());
            request.Reject();
            return;
        }

        // Accept the connection.
        logger.LogInformation("Accepting new connection for user {Id} from {Ip}.",
            accountId, request.RemoteEndPoint.ToString());
        var peer = request.Accept();
        loginTracker.AssociateConnection(accountId, peer.Id);
        connectionManager.CreateConnection(peer, sharedSecret);
    }

    /// <summary>
    ///     Attempts the login handoff for the new connection.
    /// </summary>
    /// <param name="request">New connection request.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="sharedSecret">Shared secret, or undefined if handoff unsuccessful.</param>
    /// <returns>true if the handoff was successful, false otherwise.</returns>
    private bool AttemptHandoff(ConnectionRequest request, Guid accountId,
        [NotNullWhen(true)] out byte[]? sharedSecret)
    {
        // Check for a valid login.
        sharedSecret = null;
        if (!loginHandoffTracker.TakePendingHandoff(
                accountId))
            // No valid login.
            return false;

        // Attempt the handoff.
        return sharedSecretManager.TakeSecret(accountId, out sharedSecret);
    }

    /// <summary>
    ///     Gets the account ID for the attempted connection.
    /// </summary>
    /// <param name="request">Connection request.</param>
    /// <returns>Account ID, or Guid.Empty if the request is malformed.</returns>
    private Guid GetAccountId(ConnectionRequest request)
    {
        if (request.Data.AvailableBytes == HandoffKeyBytes)
        {
            var rawId = request.Data.GetString(HandoffKeyBytes);
            return new Guid(rawId);
        }

        // Bad connection request.
        return Guid.Empty;
    }
}