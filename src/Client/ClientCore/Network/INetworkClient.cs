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
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network;

/// <summary>
///     Interface for controlling the network client.
/// </summary>
public interface INetworkClient
{
    /// <summary>
    ///     Current status of the connection.
    /// </summary>
    /// <returns>Status.</returns>
    NetworkClientState ClientState { get; }

    /// <summary>
    ///     Network connection to the server. Only valid if ClientState is Connected.
    /// </summary>
    NetworkConnection Connection { get; }

    /// <summary>
    ///     Error message. Valid only if ClientState is Failed.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    ///     Begins a connection to a remote server by making an authentication attempt with the REST server.
    /// </summary>
    /// <param name="connectionParameters">Client connection parameters.</param>
    /// <param name="loginParameters">Login parameters.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if ClientState is not Disconnected.
    /// </exception>
    void BeginConnection(ClientConnectionParameters connectionParameters, LoginParameters loginParameters);

    /// <summary>
    ///     Continues a connection to a remote server following successful authentication.
    /// </summary>
    /// <param name="loginResponse">Successful login response.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if ClientState is not Connecting.
    /// </exception>
    void ContinueConnection(LoginResponse loginResponse);

    /// <summary>
    ///     Ends the current connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if ClientState is not Connecting or Connected.
    /// </exception>
    void EndConnection();

    /// <summary>
    ///     Resets the error state, transitioning from Failed to Disconnected.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if ClientState is not Failed.
    /// </exception>
    void ResetError();
}