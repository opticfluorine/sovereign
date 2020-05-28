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
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.WorldGen.Configuration
{

    /// <summary>
    /// Loads WorldGen configurations from YAML files.
    /// </summary>
    public sealed class WorldGenConfigurationLoader
    {

        /// <summary>
        /// YAML deserializer.
        /// </summary>
        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        /// <summary>
        /// Loads a WorldGen configuration from a YAML fie.
        /// </summary>
        /// 
        /// Note that the contents of the file are not validated at this stage.
        /// 
        /// <param name="filename">Path to configuration file.</param>
        /// <returns>WorldGen configuration.</returns>
        /// <exception cref="WorldGenConfigurationException">
        /// Thrown if the configuration file cannot be read.
        /// </exception>
        public WorldGenConfiguration LoadConfiguration(string filename)
        {
            /* Deserialize configuration. */
            WorldGenConfiguration config;
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    config = deserializer.Deserialize<WorldGenConfiguration>(reader);
                }
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();

                throw new WorldGenConfigurationException(sb.ToString(), e);
            }

            return config;
        }

    }

}
