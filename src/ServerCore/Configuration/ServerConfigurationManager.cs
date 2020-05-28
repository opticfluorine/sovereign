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

using Sovereign.EngineCore.Resources;
using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ServerCore.Configuration
{

    /// <summary>
    /// Responsible for loading the server configuration at runtime.
    /// </summary>
    public sealed class ServerConfigurationManager : IServerConfigurationManager
    {

        public ServerConfiguration ServerConfiguration => serverConfiguration;

        private readonly ServerConfiguration serverConfiguration;

        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        /// <summary>
        /// Server configuration filename.
        /// </summary>
        private const string configFilename = "ServerConfiguration.yaml";

        public ServerConfigurationManager(IResourcePathBuilder resourcePathBuilder)
        {
            var cfgPath = resourcePathBuilder
                .BuildPathToResource(ResourceType.Configuration, configFilename);
            using (var reader = new StreamReader(cfgPath))
            {
                serverConfiguration = deserializer.Deserialize<ServerConfiguration>(reader);
            }
        }

    }

}
