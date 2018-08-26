using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.WorldLib.Material
{

    /// <summary>
    /// IoC installer for materials.
    /// </summary>
    public class MaterialInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* MaterialDefinitionsValidator. */
            container.Register(Component.For<MaterialDefinitionsValidator>()
                .LifestyleTransient());

            /* MaterialDefinitionsLoader. */
            container.Register(Component.For<MaterialDefinitionsLoader>()
                .LifestyleTransient());
        }

    }

}
