/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Sovereign.Persistence.Configuration;
using Sovereign.Persistence.Database.Sqlite;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Responsible for managing
///     <see cref="IPersistenceProvider">IPersistenceProvider</see>
///     implementations.
/// </summary>
public sealed class PersistenceProviderManager
{
    public PersistenceProviderManager(IPersistenceConfiguration configuration)
    {
        switch (configuration.DatabaseType)
        {
            case DatabaseType.Sqlite:
                PersistenceProvider = new SqlitePersistenceProvider(configuration);
                break;

            case DatabaseType.Postgres:
                throw new NotImplementedException();

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Currently active persistence provider.
    /// </summary>
    public IPersistenceProvider PersistenceProvider { get; private set; }
}