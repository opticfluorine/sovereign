using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Engine8.EngineUtil.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Components
{

    /// <summary>
    /// IoC installer for core component services.
    /// </summary>
    public class ComponentInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* ComponentManager. */
            container.Register(Component.For<ComponentManager>()
                .LifestyleSingleton());

            /* Component updaters. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn(typeof(BaseComponentCollection<>))
                .WithServiceBase()
                .WithServiceSelf()
                .WithServices(typeof(IComponentUpdater))
                .LifestyleSingleton()
                .AllowMultipleMatches());
        }

    }

}
