/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
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
