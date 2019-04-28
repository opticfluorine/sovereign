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

using Sovereign.Persistence.Configuration;
using Sovereign.Persistence.Database.Sqlite;
using Sovereign.ServerCore.Configuration;
using System;

namespace Sovereign.Persistence.Database
{

    /// <summary>
    /// Responsible for managing
    /// <see cref="IPersistenceProvider">IPersistenceProvider</see> 
    /// implementations.
    /// </summary>
    public sealed class PersistenceProviderManager
    {
        private readonly IPersistenceConfiguration configuration;
        private readonly SqlitePersistenceProvider sqlitePersistenceProvider;

        /// <summary>
        /// Currently active persistence provider.
        /// </summary>
        public IPersistenceProvider PersistenceProvider { get; private set; }

        public PersistenceProviderManager(IPersistenceConfiguration configuration,
            SqlitePersistenceProvider sqlitePersistenceProvider)
        {
            this.configuration = configuration;
            this.sqlitePersistenceProvider = sqlitePersistenceProvider;

            Initialize();
        }

        /// <summary>
        /// Initializes the provider manager based on the current configuration.
        /// </summary>
        private void Initialize()
        {
            switch (configuration.DatabaseType)
            {
                case DatabaseType.Sqlite:
                    PersistenceProvider = sqlitePersistenceProvider;
                    break;

                case DatabaseType.Postgres:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

    }

}
