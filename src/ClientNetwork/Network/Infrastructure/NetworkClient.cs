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
