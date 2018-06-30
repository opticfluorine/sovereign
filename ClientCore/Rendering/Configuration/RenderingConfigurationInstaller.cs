using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Engine8.EngineUtil.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// IoC installer for rendering configuration classes.
    /// </summary>
    public class RenderingConfigurationInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* IAdapterEnumerator. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IAdapterEnumerator>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton());

            /* AdapterSelector. */
            container.Register(Component.For<AdapterSelector>()
                .LifestyleSingleton());
        }

    }

}
