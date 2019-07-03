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
        /// Key used for HMAC.
        /// </summary>
        internal byte[] Key { get => _Key; }

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

        public void Dispose()
        {
            /* Zero the key and release the pin. */
            Array.Clear(_Key, 0, _Key.Length);
            keyHandle.Free();
        }

    }

}
