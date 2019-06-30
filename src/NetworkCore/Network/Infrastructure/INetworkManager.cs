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

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Delegate type for new connection requests.
    /// </summary>
    public delegate void OnConnectionRequest();

    /// <summary>
    /// Delegate type for new connections.
    /// </summary>
    public delegate void OnConnected();

    /// <summary>
    /// Delegate type for disconnections.
    /// </summary>
    public delegate void OnDisconnected();

   /// <summary>
    /// Delegate type for network errors.
    /// </summary>
    public delegate void OnNetworkError();

    /// <summary>
    /// Delegate type for received packets.
    /// </summary>
    public delegate void OnNetworkReceive();

    /// <summary>
    /// Delegate type for received packets not associated with a connection.
    /// </summary>
    public delegate void OnNetworkReceiveUnconnected();

    /// <summary>
    /// Interface for managing the network for a client or server.
    /// </summary>
    public interface INetworkManager : IDisposable
    {

        /// <summary>
        /// Event invoked when a new connection request is received.
        /// </summary>
        event OnConnectionRequest OnConnectionRequest;

        /// <summary>
        /// Event invoked when a connection is established.
        /// </summary>
        event OnConnected OnConnected;

        /// <summary>
        /// Event invoked when a connection is closed.
        /// </summary>
        event OnDisconnected OnDisconnected;

        /// <summary>
        /// Event invoked when a network error occurs.
        /// </summary>
        event OnNetworkError OnNetworkError;

        /// <summary>
        /// Event invoked when a packet is received.
        /// </summary>
        event OnNetworkReceive OnNetworkReceive;

        /// <summary>
        /// Event invoked when a packet is received without an associated
        /// connection.
        /// </summary>
        event OnNetworkReceiveUnconnected OnNetworkReceiveUnconnected;

        /// <summary>
        /// Initializes the network manager.
        /// </summary>
        void Initialize();

    }

}
