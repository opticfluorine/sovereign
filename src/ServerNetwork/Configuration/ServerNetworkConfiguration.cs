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

        public ushort Port => serverConfiguration.Network.Port;

        public string RestHostname => serverConfiguration.Network.RestHostname;

        public ushort RestPort => serverConfiguration.Network.RestPort;

        public ServerNetworkConfiguration(IServerConfigurationManager configurationManager)
        {
            serverConfiguration = configurationManager.ServerConfiguration;
        }

    }

}
