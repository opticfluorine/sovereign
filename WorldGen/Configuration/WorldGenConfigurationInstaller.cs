using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldGen.Configuration
{

    /// <summary>
    /// IoC installer for WorldGen configuration helper classes.
    /// </summary>
    public class WorldGenConfigurationInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* WorldGenConfigurationLoader. */
            container.Register(Component.For<WorldGenConfigurationLoader>()
                .LifestyleTransient());

            /* WorldGenConfigurationValidator. */
            container.Register(Component.For<WorldGenConfigurationValidator>()
                .LifestyleTransient());
        }

    }

}
