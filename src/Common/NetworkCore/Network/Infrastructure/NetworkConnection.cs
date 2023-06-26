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
using LiteNetLib;
using Sovereign.EngineCore.Events;

namespace Sovereign.NetworkCore.Network.Infrastructure;

/// <summary>
///     Encapsulates the connection to a single remote peer.
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///     Network peer.
    /// </summary>
    private readonly NetPeer peer;

    /// <summary>
    ///     Set of received nonces that are no longer valid for this connection.
    /// </summary>
    private readonly HashSet<uint> receivedNonces = new();

    /// <summary>
    ///     Next available outbound nonce.
    /// </summary>
    private uint nextOutboundNonce;

    /// <summary>
    ///     Creates a new connection.
    /// </summary>
    /// <param name="peer">Connected peer.</param>
    /// <param name="key">HMAC key.</param>
    public NetworkConnection(NetPeer peer, byte[] key)
    {
        this.peer = peer;
        Key = key;
    }

    /// <summary>
    ///     Unique ID number for this connection.
    /// </summary>
    public int Id => peer.Id;

    /// <summary>
    ///     Key used for HMAC.
    /// </summary>
    internal byte[] Key { get; }

    public void Dispose()
    {
        if (peer.ConnectionState == ConnectionState.Connected) peer.Disconnect();
    }

    /// <summary>
    ///     Determines whether a received nonce is valid.
    /// </summary>
    /// <param name="receivedNonce">Received nonce.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsReceivedNonceValid(uint receivedNonce)
    {
        var valid = !receivedNonces.Contains(receivedNonce);
        if (valid) receivedNonces.Add(receivedNonce);
        return valid;
    }

    /// <summary>
    ///     Gets the next usable outbound nonce.
    /// </summary>
    /// <returns>Next usable outbound nonce.</returns>
    public uint GetOutboundNonce()
    {
        return nextOutboundNonce++;
    }

    /// <summary>
    ///     Serializes and sends an event to the remote endpoint.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void SendEvent(Event ev)
    {
        // Check state.
        if (peer.ConnectionState != ConnectionState.Connected)
            throw new NetworkException("Cannot send event to disconnected peer.");

        // Serialize event.
    }

    /// <summary>
    ///     Disconnects the connection if it is currently connected.
    /// </summary>
    internal void Disconnect()
    {
        if (peer.ConnectionState == ConnectionState.Connected) peer.Disconnect();
    }
}