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

using Sovereign.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Delegate type for received packets.
    /// </summary>
    /// <param name="ev">Received event.</param>
    /// <param name="connection">Associated connection.</param>
    public delegate void OnNetworkReceive(Event ev, NetworkConnection connection);

    /// <summary>
    /// Interface for managing the network for a client or server.
    /// </summary>
    public interface INetworkManager : IDisposable
    {
        /// <summary>
        /// Event invoked when a packet is received.
        /// </summary>
        event OnNetworkReceive OnNetworkReceive;

        /// <summary>
        /// Initializes the network manager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Polls the network.
        /// </summary>
        void Poll();

    }

}
