using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.StandaloneWorldGen.Logging
{

    /// <summary>
    /// IoC installer for logging in the StandaloneWorldGen utility.
    /// </summary>
    public class StandaloneWorldGenLoggingInstaller : IWindsorInstaller
    {

        /// <summary>
        /// Name of the logging configuration file.
        /// </summary>
        private const string LogConfigFilepath = "Logging.xml";

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* Configure log4net. */
            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>()
                .WithConfig(LogConfigFilepath));
        }

    }

}
