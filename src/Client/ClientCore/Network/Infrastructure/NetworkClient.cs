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

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Network client implementation.
/// </summary>
public sealed class NetworkClient : INetworkClient
{
    private readonly ClientNetworkManager clientNetworkManager;

    public NetworkClient(ClientNetworkManager clientNetworkManager)
    {
        this.clientNetworkManager = clientNetworkManager;
    }

    public NetworkClientState ClientState => clientNetworkManager.ClientState;

    public NetworkConnection Connection
    {
        get
        {
            if (clientNetworkManager.Connection == null) throw new InvalidOperationException("Client not connected.");
            return clientNetworkManager.Connection;
        }
    }

    public string ErrorMessage => clientNetworkManager.ErrorMessage;

    public void BeginConnection(ClientConnectionParameters connectionParameters, LoginParameters loginParameters)
    {
        clientNetworkManager.BeginConnection(connectionParameters, loginParameters);
    }

    public void ContinueConnection(LoginResponse loginResponse)
    {
    }

    public void EndConnection()
    {
        clientNetworkManager.EndConnection();
    }

    public void ResetError()
    {
        clientNetworkManager.ResetError();
    }
}