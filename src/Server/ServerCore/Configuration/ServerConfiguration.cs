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

namespace Sovereign.ServerCore.Configuration;

/// <summary>
///     Full description of the database configuration.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    ///     Database type.
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.Sqlite;

    /// <summary>
    ///     Database host for database servers, or data source for sqlite.
    /// </summary>
    public string Host { get; set; } = "Data/sovereign.db";

    /// <summary>
    ///     Database port for database servers. Not used for sqlite.
    /// </summary>
    public ushort Port { get; set; } = 5432;

    /// <summary>
    ///     Database name. Not used for sqlite.
    /// </summary>
    public string Database { get; set; } = "sovereign";

    /// <summary>
    ///     Database username. Not used for sqlite.
    /// </summary>
    public string Username { get; set; } = "sovereign";

    /// <summary>
    ///     Database password. Not used for sqlite.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    ///     Whether to use connection pooling.
    /// </summary>
    public bool Pooling { get; set; } = true;

    /// <summary>
    ///     Minimum connection pool size.
    /// </summary>
    public int PoolSizeMin { get; set; } = 1;

    /// <summary>
    ///     Maximum connection pool size.
    /// </summary>
    public int PoolSizeMax { get; set; } = 100;

    /// <summary>
    ///     Maximum interval between database synchronizations,
    ///     in seconds.
    /// </summary>
    public int SyncIntervalSeconds { get; set; } = 60;

    /// <summary>
    ///     Flag indicating whether to create the database if it does not exist.
    ///     Only used for SQLite.
    /// </summary>
    public bool CreateIfMissing { get; set; } = true;
}

/// <summary>
///     Full description of the server-side network configuration.
/// </summary>
public sealed class NetworkOptions
{
    /// <summary>
    ///     IPv4 network interface to bind.
    /// </summary>
    public string NetworkInterfaceIPv4 { get; set; } = "0.0.0.0";

    /// <summary>
    ///     IPv6 network interface to bind.
    /// </summary>
    public string NetworkInterfaceIPv6 { get; set; } = "::0";

    /// <summary>
    ///     Server hostname to use for connection handoff.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    ///     Server port.
    /// </summary>
    public ushort Port { get; set; } = 12820;

    /// <summary>
    ///     REST server hostname.
    /// </summary>
    public string RestHostname { get; set; } = "127.0.0.1";

    /// <summary>
    ///     REST server port.
    /// </summary>
    public ushort RestPort { get; set; } = 8080;

    /// <summary>
    ///     Automatic ping interval, in milliseconds.
    /// </summary>
    public uint PingIntervalMs { get; set; } = 10000;

    /// <summary>
    ///     Timeout period (in milliseconds) after which a client will be disconnected from
    ///     the event server if the server has not received any messages.
    /// </summary>
    public uint ConnectionTimeoutMs { get; set; } = 30000;

    /// <summary>
    ///     Maximum number of entities to synchronize to the client in a single event.
    ///     Greater values improve throughput at the cost of increased latency due to a higher packet loss rate.
    /// </summary>
    public int EntitySyncBatchSize { get; set; } = 16;
}

/// <summary>
///     Full description of the new players configuration.
/// </summary>
public sealed class NewPlayersOptions
{
    /// <summary>
    ///     Whether to grant the admin role to new players by default.
    /// </summary>
    public bool AdminByDefault { get; set; } = false;
}

/// <summary>
///     Full description of the scripting engine configuration.
/// </summary>
public sealed class ScriptingOptions
{
    /// <summary>
    ///     Base directory where scripts are located.
    /// </summary>
    public string ScriptDirectory { get; set; } = "Data/Scripts";

    /// <summary>
    ///     Maximum number of directories (including the base directory) to recurse into when
    ///     searching for scripts.
    /// </summary>
    public uint MaxDirectoryDepth { get; set; } = 5;
}

/// <summary>
///     Full description of the accounts configuration.
/// </summary>
public sealed class AccountsOptions
{
    /// <summary>
    ///     Maximum number of failed login attempts before access to
    ///     an account is temporarily disabled.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 10;

    /// <summary>
    ///     Length of the login denial period, in seconds. The failed
    ///     login attempt count is reset after this amount of time has
    ///     elapsed since the last failed login attempt.
    /// </summary>
    public int LoginDenialPeriodSeconds { get; set; } = 1800;

    /// <summary>
    ///     Minimum length of all new passwords.
    /// </summary>
    public int MinimumPasswordLength { get; set; } = 8;

    /// <summary>
    ///     Length of the connection handoff period following a successful
    ///     login. The connection may only be established during the handoff,
    ///     beyond which the handoff will be invalidated.
    /// </summary>
    public int HandoffPeriodSeconds { get; set; } = 30;
}