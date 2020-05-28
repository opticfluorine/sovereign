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

        /// <summary>
        /// Removes the given connection, closing it if needed and disposing the
        /// associated resources.
        /// </summary>
        /// <param name="connection">Connection to remove.</param>
        public void RemoveConnection(NetworkConnection connection)
        {
            connections.Remove(connection.Id);
            connection.Disconnect();
            connection.Dispose();
        }

        /// <summary>
        /// Convenience method that removes the connection with the given ID.
        /// </summary>
        /// <param name="id">Connection ID.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no connection is known with the given ID.
        /// </exception>
        public void RemoveConnection(int id)
        {
            var conn = GetConnection(id);
            RemoveConnection(conn);
        }

    }

}
