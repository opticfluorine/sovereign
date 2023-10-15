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
///     Persistence configuration.
/// </summary>
public interface IPersistenceConfiguration
{
    /// <summary>
    ///     Database type.
    /// </summary>
    DatabaseType DatabaseType { get; }

    /// <summary>
    ///     Database hostname for database servers, or data source for sqlite.
    /// </summary>
    string Host { get; }

    /// <summary>
    ///     Database port. Not used for sqlite.
    /// </summary>
    ushort Port { get; }

    /// <summary>
    ///     Database name. Not used for sqlite.
    /// </summary>
    string Database { get; }

    /// <summary>
    ///     Username. Not used for sqlite.
    /// </summary>
    string Username { get; }

    /// <summary>
    ///     Password. Not used for sqlite.
    /// </summary>
    string Password { get; }

    /// <summary>
    ///     Whether to use connection pooling.
    /// </summary>
    bool Pooling { get; }

    /// <summary>
    ///     Minimum size of connection pool.
    /// </summary>
    int PoolSizeMin { get; }

    /// <summary>
    ///     Maximum size of connection pool.
    /// </summary>
    int PoolSizeMax { get; }

    /// <summary>
    ///     Maximum interval between persistence synchronizations
    ///     in seconds.
    /// </summary>
    int SyncIntervalSeconds { get; }
}