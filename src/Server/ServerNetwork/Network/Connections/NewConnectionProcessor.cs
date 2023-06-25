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
using Castle.Core.Logging;
using LiteNetLib;
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
    private const int HANDOFF_KEY_BYTES = 38;

    private readonly NetworkConnectionManager connectionManager;
    private readonly LoginHandoffTracker loginHandoffTracker;
    private readonly SharedSecretManager sharedSecretManager;

    public NewConnectionProcessor(NetworkConnectionManager connectionManager,
        LoginHandoffTracker loginHandoffTracker,
        SharedSecretManager sharedSecretManager)
    {
        this.connectionManager = connectionManager;
        this.loginHandoffTracker = loginHandoffTracker;
        this.sharedSecretManager = sharedSecretManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
            Logger.InfoFormat("Rejecting new connection from {0}: malformed.",
                request.RemoteEndPoint.ToString());
            request.Reject();
            return;
        }

        // Attempt handoff for the account.
        if (!AttemptHandoff(request, accountId, out var sharedSecret))
        {
            // Failed.
            Logger.InfoFormat("Rejecting new connection from {0}: bad handoff.",
                request.RemoteEndPoint.ToString());
            request.Reject();
            return;
        }

        // Accept the connection.
        Logger.InfoFormat("Accepting new connection for user {0} from {1}.",
            null, request.RemoteEndPoint.ToString());
        connectionManager.CreateConnection(request.Accept(), sharedSecret);
    }

    /// <summary>
    ///     Attempts the login handoff for the new connection.
    /// </summary>
    /// <param name="request">New connection request.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="sharedSecret">Shared secret, or undefined if handoff unsuccessful.</param>
    /// <returns>true if the handoff was successful, false otherwise.</returns>
    private bool AttemptHandoff(ConnectionRequest request, Guid accountId,
        out byte[] sharedSecret)
    {
        // Check for a valid login.
        sharedSecret = null;
        if (!loginHandoffTracker.TakePendingHandoff(
                accountId))
            // No valid login.
            return false;

        // Attempt the handoff.
        if (!sharedSecretManager.TakeSecret(accountId, out sharedSecret))
            // Handoff expired.
            return false;

        // Handoff successful.
        return true;
    }

    /// <summary>
    ///     Gets the account ID for the attempted connection.
    /// </summary>
    /// <param name="request">Connection request.</param>
    /// <returns>Account ID, or Guid.Empty if the request is malformed.</returns>
    private Guid GetAccountId(ConnectionRequest request)
    {
        if (request.Data.AvailableBytes == HANDOFF_KEY_BYTES)
        {
            var rawId = request.Data.GetString(HANDOFF_KEY_BYTES);
            return new Guid(rawId);
        }

        // Bad connection request.
        return Guid.Empty;
    }
}