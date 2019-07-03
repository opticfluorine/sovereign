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
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Manages the set of all network connections.
    /// </summary>
    public sealed class NetworkConnectionManager
    {

        /// <summary>
        /// Map from connection ID to connection object.
        /// </summary>
        private readonly IDictionary<int, NetworkConnection> connections
            = new Dictionary<int, NetworkConnection>();

        /// <summary>
        /// Gets a connection by ID.
        /// </summary>
        /// <param name="connectionId">Connection ID.</param>
        /// <returns>Network connection.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the connection ID is not recognized.
        /// </exception>
        public NetworkConnection GetConnection(int connectionId)
        {
            if (!connections.ContainsKey(connectionId))
            {
                throw new KeyNotFoundException("Unknown connection ID.");
            }

            return connections[connectionId];
        }
        
        /// <summary>
        /// Creates a connection from a NetPeer.
        /// </summary>
        /// <param name="peer">Connected peer.</param>
        /// <param name="key">HMAC key for the connection.
        /// The passed array is zeroed by this operation.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if a connection has already been created for the given peer.
        /// </exception>
        public NetworkConnection CreateConnection(NetPeer peer, byte[] key)
        {
            if (connections.ContainsKey(peer.Id))
            {
                throw new ArgumentException("Connection already created for peer.");
            }

            var conn = new NetworkConnection(peer, key);
            connections[peer.Id] = conn;
            return conn;
        }

        public void RemoveConnection(NetworkConnection connection)
        {
            // TODO
        }

    }

}
