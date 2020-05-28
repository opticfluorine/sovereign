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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Network
{

    /// <summary>
    /// Interface for controlling the network client.
    /// </summary>
    public interface INetworkClient
    {

        /// <summary>
        /// Begins a connection to a remote server.
        /// </summary>
        /// <param name="host">Server host.</param>
        /// <param name="port">Server port.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if ClientState is not Disconnected.
        /// </exception>
        void BeginConnection(string host, ushort port);

        /// <summary>
        /// Ends the current connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if ClientState is not Connecting or Connected.
        /// </exception>
        void EndConnection();

        /// <summary>
        /// Current status of the connection.
        /// </summary>
        /// <returns>Status.</returns>
        NetworkClientState ClientState { get; }

        /// <summary>
        /// Error message. Valid only if ClientState is Failed.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Resets the error state, transitioning from Failed to Disconnected.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if ClientState is not Failed.
        /// </exception>
        void ResetError();

    }

}
