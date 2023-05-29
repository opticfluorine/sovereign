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
        /// <param name="connectionParameters">Client connection parameters.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if ClientState is not Disconnected.
        /// </exception>
        void BeginConnection(ClientConnectionParameters connectionParameters);

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
