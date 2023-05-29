/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

namespace Sovereign.ClientNetwork.Network
{

    /// <summary>
    /// Describes a set of connection parameters for a server.
    /// </summary>
    public sealed class ClientConnectionParameters
    {

        /// <summary>
        /// Server hostname.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Server port.
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// REST server hostname. Typically the same as Host.
        /// </summary>
        public string RestHost { get; private set; }

        /// <summary>
        /// REST server port.
        /// </summary>
        public ushort RestPort { get; private set; }

        /// <summary>
        /// Whether the REST server is using a TLS-encrypted connection.
        /// </summary>
        public bool RestTls { get; private set; }

        public ClientConnectionParameters(string host, ushort port, string restHost, ushort restPort, bool restTls)
        {
            Host = host;
            Port = port;
            RestHost = restHost;
            RestPort = restPort;
            RestTls = restTls;
        }
    }

}
