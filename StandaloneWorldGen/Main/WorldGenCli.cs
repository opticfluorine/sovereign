using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.StandaloneWorldGen.Main
{

    /// <summary>
    /// Command-line interface to WorldGen.
    /// </summary>
    public class WorldGenCli
    {

        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        private string configFilePath;

        /// <summary>
        /// Path to the output file.
        /// </summary>
        private string outputFilePath;

        /// <summary>
        /// Runs the application.
        /// </summary>
        public void Run(string[] args)
        {
            /* Parse command-line arguments. */
            ParseArguments(args);
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void ParseArguments(string[] args)
        {
            /* Check that arguments are specified correctly. */
            if (args.Length != 2)
                PrintHelpInfo();

            /* Parse arguments. */
            configFilePath = args[0];
            outputFilePath = args[1];
        }

        /// <summary>
        /// Prints help information and exits.
        /// </summary>
        private void PrintHelpInfo()
        {
            Console.Write("usage: ");
            Console.Write(AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine(" configFilePath outputFilePath");

            Environment.Exit(1);
        }

    }

}
