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

using Sovereign.ServerCore.Configuration;

namespace Sovereign.Persistence.Configuration
{

    /// <summary>
    /// Persistence configuration.
    /// </summary>
    public interface IPersistenceConfiguration
    {

        /// <summary>
        /// Database type.
        /// </summary>
        DatabaseType DatabaseType { get; }

        /// <summary>
        /// Database hostname for database servers, or data source for sqlite.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Database port. Not used for sqlite.
        /// </summary>
        ushort Port { get; }

        /// <summary>
        /// Database name. Not used for sqlite.
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Username. Not used for sqlite.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Password. Not used for sqlite.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Whether to use connection pooling.
        /// </summary>
        bool Pooling { get; }

        /// <summary>
        /// Minimum size of connection pool.
        /// </summary>
        int PoolSizeMin { get; }

        /// <summary>
        /// Maximum size of connection pool.
        /// </summary>
        int PoolSizeMax { get; }

        /// <summary>
        /// Maximum interval between persistence synchronizations
        /// in seconds.
        /// </summary>
        int SyncIntervalSeconds { get; }

    }

}
