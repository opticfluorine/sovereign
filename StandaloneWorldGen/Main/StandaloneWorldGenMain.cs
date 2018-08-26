using Engine8.EngineUtil.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.StandaloneWorldGen.Main
{

    /// <summary>
    /// Main class for the standalone world generator utility.
    /// </summary>
    class StandaloneWorldGenMain
    {

        static void Main(string[] args)
        {
            /* Start up the IoC container. */
            try
            {
                var iocContainer = IoCUtil.InitializeIoC();
                var cli = iocContainer.Resolve<WorldGenCli>();
                cli.Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Environment.Exit(1);
            }
        }

    }

}
