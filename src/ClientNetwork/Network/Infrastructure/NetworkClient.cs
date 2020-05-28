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

using Sovereign.ClientCore.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientNetwork.Network.Infrastructure
{

    /// <summary>
    /// Network client implementation.
    /// </summary>
    public sealed class NetworkClient : INetworkClient
    {
        private readonly ClientNetworkManager clientNetworkManager;

        public NetworkClient(ClientNetworkManager clientNetworkManager)
        {
            this.clientNetworkManager = clientNetworkManager;
        }

        public NetworkClientState ClientState => clientNetworkManager.ClientState;

        public string ErrorMessage => clientNetworkManager.ErrorMessage;

        public void BeginConnection(string host, ushort port)
        {
            clientNetworkManager.BeginConnection(host, port);
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
}
