using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.StandaloneWorldGen.Main
{

    /// <summary>
    /// IoC installer for StandaloneWorldGen main classes.
    /// </summary>
    public class StandaloneWorldGenMainInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* WorldGenCli. */
            container.Register(Component.For<WorldGenCli>()
                .LifestyleTransient());
        }

    }
}
