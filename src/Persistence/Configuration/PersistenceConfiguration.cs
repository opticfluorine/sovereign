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
    /// Persistence configuration that proxies the server runtime configuration.
    /// </summary>
    public sealed class PersistenceConfiguration : IPersistenceConfiguration
    {
        private readonly ServerConfiguration serverConfiguration;

        public DatabaseType DatabaseType => serverConfiguration.Database.DatabaseType;

        public string Host => serverConfiguration.Database.Host;

        public ushort Port => serverConfiguration.Database.Port;

        public string Database => serverConfiguration.Database.Database;

        public string Username => serverConfiguration.Database.Username;

        public string Password => serverConfiguration.Database.Password;

        public bool Pooling => serverConfiguration.Database.Pooling;

        public int PoolSizeMin => serverConfiguration.Database.PoolSizeMin;

        public int PoolSizeMax => serverConfiguration.Database.PoolSizeMax;

        public int SyncIntervalSeconds => serverConfiguration.Database.SyncIntervalSeconds;

        public PersistenceConfiguration(IServerConfigurationManager configurationManager)
        {
            serverConfiguration = configurationManager.ServerConfiguration;
        }

    }

}
