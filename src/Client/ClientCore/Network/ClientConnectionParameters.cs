/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

namespace Sovereign.ClientCore.Network;

/// <summary>
///     Describes a set of connection parameters for a server.
/// </summary>
public sealed class ClientConnectionParameters
{
    public const string DefaultHost = "127.0.0.1";

    public const ushort DefaultPort = 12820;

    public const string DefaultRestHost = "127.0.0.1";

    public const ushort DefaultRestPort = 8080;

    public const bool DefaultRestTls = false;

    public ClientConnectionParameters() : this(DefaultHost, DefaultPort, DefaultRestHost,
        DefaultRestPort, DefaultRestTls)
    {
    }

    public ClientConnectionParameters(string host, ushort port, string restHost, ushort restPort, bool restTls)
    {
        Host = host;
        Port = port;
        RestHost = restHost;
        RestPort = restPort;
        RestTls = restTls;
    }

    /// <summary>
    ///     Server hostname.
    /// </summary>
    public string Host { get; private set; }

    /// <summary>
    ///     Server port.
    /// </summary>
    public ushort Port { get; private set; }

    /// <summary>
    ///     REST server hostname. Typically the same as Host.
    /// </summary>
    public string RestHost { get; private set; }

    /// <summary>
    ///     REST server port.
    /// </summary>
    public ushort RestPort { get; private set; }

    /// <summary>
    ///     Whether the REST server is using a TLS-encrypted connection.
    /// </summary>
    public bool RestTls { get; private set; }
}