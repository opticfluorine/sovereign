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

using Castle.Core.Logging;
using Sovereign.WorldGen.Configuration;
using Sovereign.WorldLib.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.StandaloneWorldGen.Main
{

    /// <summary>
    /// Command-line interface to WorldGen.
    /// </summary>
    public class WorldGenCli
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Material definitions loader.
        /// </summary>
        private readonly MaterialDefinitionsLoader materialDefinitionsLoader;

        /// <summary>
        /// WorldGen configuration loader.
        /// </summary>
        private readonly WorldGenConfigurationLoader worldGenConfigurationLoader;

        /// <summary>
        /// Path to the material definitions file.
        /// </summary>
        private string materialDefinitionsFilePath;

        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        private string configFilePath;

        /// <summary>
        /// Path to the output file.
        /// </summary>
        private string outputFilePath;

        public WorldGenCli(MaterialDefinitionsLoader materialDefinitionsLoader,
            WorldGenConfigurationLoader worldGenConfigurationLoader)
        {
            this.materialDefinitionsLoader = materialDefinitionsLoader;
            this.worldGenConfigurationLoader = worldGenConfigurationLoader;
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        public void Run(string[] args)
        {
            /* Parse command-line arguments. */
            ParseArguments(args);

            /* Load material definitions. */
            MaterialDefinitions materialDefinitions;
            try
            {
                materialDefinitions = materialDefinitionsLoader.LoadDefinitions(materialDefinitionsFilePath);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to load material definitions.", e);
                Environment.Exit(1);
            }

            /* Load WorldGen configuration. */
            WorldGenConfiguration worldGenConfiguration;
            try
            {
                worldGenConfiguration = worldGenConfigurationLoader.LoadConfiguration(configFilePath);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to load WorldGen configuration.", e);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void ParseArguments(string[] args)
        {
            /* Check that arguments are specified correctly. */
            if (args.Length != 3)
                PrintHelpInfo();

            /* Parse arguments. */
            materialDefinitionsFilePath = args[0];
            configFilePath = args[1];
            outputFilePath = args[2];
        }

        /// <summary>
        /// Prints help information and exits.
        /// </summary>
        private void PrintHelpInfo()
        {
            Console.Write("usage: ");
            Console.Write(AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine(" materialDefinitionsFilePath configFilePath outputFilePath");

            Environment.Exit(1);
        }

    }

}
