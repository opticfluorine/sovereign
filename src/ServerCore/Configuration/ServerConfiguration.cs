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

namespace Sovereign.ServerCore.Configuration
{

    /// <summary>
    /// Full description of the server runtime configuration.
    /// </summary>
    public sealed class ServerConfiguration
    {

        /// <summary>
        /// Database configuration settings.
        /// </summary>
        public DatabaseRecord Database { get; set; }

        /// <summary>
        /// Server-side network configuration settings.
        /// </summary>
        public NetworkRecord Network { get; set; }

        /// <summary>
        /// World configuration settings.
        /// </summary>
        public WorldRecord World { get; set; }

        /// <summary>
        /// Full description of the database configuration.
        /// </summary>
        public sealed class DatabaseRecord
        {

            /// <summary>
            /// Database type.
            /// </summary>
            public DatabaseType DatabaseType { get; set; }

            /// <summary>
            /// Database host for database servers, or data source for sqlite.
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// Database port for database servers. Not used for sqlite.
            /// </summary>
            public ushort Port { get; set; } = 5432;

            /// <summary>
            /// Database name. Not used for sqlite.
            /// </summary>
            public string Database { get; set; } = "sovereign";

            /// <summary>
            /// Database username. Not used for sqlite.
            /// </summary>
            public string Username { get; set; } = "sovereign";

            /// <summary>
            /// Database password. Not used for sqlite.
            /// </summary>
            public string Password { get; set; } = "";

            /// <summary>
            /// Whether to use connection pooling.
            /// </summary>
            public bool Pooling { get; set; } = true;

            /// <summary>
            /// Minimum connection pool size.
            /// </summary>
            public int PoolSizeMin { get; set; } = 1;

            /// <summary>
            /// Maximum connection pool size.
            /// </summary>
            public int PoolSizeMax { get; set; } = 100;

            /// <summary>
            /// Maximum interval between database synchronizations,
            /// in seconds.
            /// </summary>
            public int SyncIntervalSeconds { get; set; } = 60;

        }

        /// <summary>
        /// Full description of the server-side network configuration.
        /// </summary>
        public sealed class NetworkRecord
        {

            /// <summary>
            /// IPv4 network interface to bind.
            /// </summary>
            public string NetworkInterfaceIPv4 { get; set; } = "0.0.0.0";

            /// <summary>
            /// IPv6 network interface to bind.
            /// </summary>
            public string NetworkInterfaceIPv6 { get; set; } = "::0";

            /// <summary>
            /// Server port.
            /// </summary>
            public ushort Port { get; set; } = 12820;

            /// <summary>
            /// REST server hostname.
            /// </summary>
            public string RestHostname { get; set; } = "127.0.0.1";

            /// <summary>
            /// REST server port.
            /// </summary>
            public ushort RestPort { get; set; } = 8080;

        }

        /// <summary>
        /// Full description of the world configuration.
        /// </summary>
        public sealed class WorldRecord
        {

            /// <summary>
            /// Length, in blocks, of each discrete chunk of the world.
            /// </summary>
            public int ChunkSize { get; set; } = 32;

        }

    }

}
