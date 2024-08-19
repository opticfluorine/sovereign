// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Castle.Core.Logging;
using Sovereign.EngineCore.Resources;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Configuration;

/// <summary>
///     Responsible for loading the client configuration at runtime.
/// </summary>
public class ClientConfigurationManager
{
    /// <summary>
    ///     Configuration filename.
    /// </summary>
    private const string Filename = "ClientConfiguration.yaml";

    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public ClientConfigurationManager(IResourcePathBuilder pathBuilder, ILogger logger)
    {
        try
        {
            var cfgPath = pathBuilder.BuildPathToResource(
                ResourceType.Configuration, Filename);
            using (var reader = new StreamReader(cfgPath))
            {
                ClientConfiguration = deserializer.Deserialize<ClientConfiguration>(reader);
            }
        }
        catch (Exception e)
        {
            logger.Fatal("Error loading configuration file.", e);
            throw;
        }
    }

    /// <summary>
    ///     Client configuration.
    /// </summary>
    public ClientConfiguration ClientConfiguration { get; }
}