/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.ServerNetwork.Configuration
{

    /// <summary>
    /// Interface for server-side network configuration.
    /// </summary>
    public interface IServerNetworkConfiguration
    {

        /// <summary>
        /// Server IPv4 network interface.
        /// </summary>
        string NetworkInterfaceIPv4 { get; }

        /// <summary>
        /// Server IPv6 network interface.
        /// </summary>
        string NetworkInterfaceIPv6 { get; }

        /// <summary>
        /// Server hostname to use for connection handoff.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Server port.
        /// </summary>
        ushort Port { get; }

        /// <summary>
        /// REST server hostname.
        /// </summary>
        string RestHostname { get; }

        /// <summary>
        /// REST server port.
        /// </summary>
        ushort RestPort { get; }

    }

}
