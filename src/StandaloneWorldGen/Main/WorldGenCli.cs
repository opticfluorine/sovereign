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
