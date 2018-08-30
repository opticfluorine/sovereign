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
        private readonly Deserializer deserializer = new DeserializerBuilder()
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
