/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Encapsulates the connection to a single remote peer.
    /// </summary>
    public sealed class NetworkConnection : IDisposable
    {

        /// <summary>
        /// Unique ID number for this connection.
        /// </summary>
        public int Id { get => peer.Id; }

        /// <summary>
        /// Key used for HMAC.
        /// </summary>
        internal byte[] Key { get => _Key; }

        private readonly NetPeer peer;

        /// <summary>
        /// Backing array for the HMAC key.
        /// </summary>
        private readonly byte[] _Key;

        /// <summary>
        /// GC handle used to pin the HMAC key.
        /// </summary>
        private readonly GCHandle keyHandle;

        /// <summary>
        /// Set of received nonces that are no longer valid for this connection.
        /// </summary>
        private readonly ISet<uint> receivedNonces = new HashSet<uint>();

        /// <summary>
        /// Next available outbound nonce.
        /// </summary>
        private uint nextOutboundNonce = 0;

        /// <summary>
        /// Creates a new connection.
        /// </summary>
        /// <param name="peer">Connected peer.</param>
        /// <param name="key">HMAC key. The passed array is erased by the constructor.</param>
        public NetworkConnection(NetPeer peer, byte[] key)
        {
            /* Set up. */
            this.peer = peer;

            /* Allocate and pin the key memory. */
            _Key = new byte[key.Length];
            keyHandle = GCHandle.Alloc(_Key, GCHandleType.Pinned);
            
            /* Copy the key and clear the original array. */
            Array.Copy(key, Key, key.Length);
            Array.Clear(key, 0, key.Length);
        }

        /// <summary>
        /// Determines whether a received nonce is valid.
        /// </summary>
        /// <param name="receivedNonce">Received nonce.</param>
        /// <returns>true if valid, false otherwise.</returns>
        public bool IsReceivedNonceValid(uint receivedNonce)
        {
            var valid = !receivedNonces.Contains(receivedNonce);
            if (valid)
            {
                receivedNonces.Add(receivedNonce);
            }
            return valid;
        }

        /// <summary>
        /// Gets the next usable outbound nonce.
        /// </summary>
        /// <returns>Next usable outbound nonce.</returns>
        public uint GetOutboundNonce()
        {
            return nextOutboundNonce++;
        }

        /// <summary>
        /// Disconnects the connection if it is currently connected.
        /// </summary>
        internal void Disconnect()
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                peer.Disconnect();
            }
        }

        public void Dispose()
        {
            /* Zero the key and release the pin. */
            Array.Clear(_Key, 0, _Key.Length);
            keyHandle.Free();
        }

    }

}
