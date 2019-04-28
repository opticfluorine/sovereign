/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
