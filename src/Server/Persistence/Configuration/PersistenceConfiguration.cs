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

using Sovereign.ServerCore.Configuration;

namespace Sovereign.Persistence.Configuration;

/// <summary>
///     Persistence configuration that proxies the server runtime configuration.
/// </summary>
public sealed class PersistenceConfiguration : IPersistenceConfiguration
{
    private readonly ServerConfiguration serverConfiguration;

    public PersistenceConfiguration(IServerConfigurationManager configurationManager)
    {
        serverConfiguration = configurationManager.ServerConfiguration;
    }

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
}