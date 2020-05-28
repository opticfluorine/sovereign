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

using Sovereign.ServerCore.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.ServerNetwork.Configuration
{

    /// <summary>
    /// Server network configuration.
    /// </summary>
    public sealed class ServerNetworkConfiguration : IServerNetworkConfiguration
    {
        private readonly ServerConfiguration serverConfiguration;

        public string NetworkInterfaceIPv4 => serverConfiguration.Network.NetworkInterfaceIPv4;

        public string NetworkInterfaceIPv6 => serverConfiguration.Network.NetworkInterfaceIPv6;

        public string Host => serverConfiguration.Network.Host;

        public ushort Port => serverConfiguration.Network.Port;

        public string RestHostname => serverConfiguration.Network.RestHostname;

        public ushort RestPort => serverConfiguration.Network.RestPort;

        public ServerNetworkConfiguration(IServerConfigurationManager configurationManager)
        {
            serverConfiguration = configurationManager.ServerConfiguration;
        }

    }

}
