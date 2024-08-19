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
using LiteNetLib;
using LiteNetLib.Utils;
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

    private readonly NetDataWriter writer = new();

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
    /// <param name="deliveryMethod">Delivery method.</param>
    /// <param name="serializer">Network serializer.</param>
    public void SendEvent(Event ev, DeliveryMethod deliveryMethod, NetworkSerializer serializer)
    {
        // Check state.
        if (peer.ConnectionState != ConnectionState.Connected)
            throw new NetworkException("Cannot send event to disconnected peer.");

        // Serialize event.
        var bytes = serializer.SerializeEvent(this, ev);

        // Send event.
        writer.Reset();
        writer.Put(bytes);
        peer.Send(writer, deliveryMethod);
    }

    /// <summary>
    ///     Disconnects the connection if it is currently connected.
    /// </summary>
    internal void Disconnect()
    {
        if (peer.ConnectionState == ConnectionState.Connected) peer.Disconnect();
    }
}