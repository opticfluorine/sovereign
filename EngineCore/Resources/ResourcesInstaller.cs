using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Engine8.EngineUtil.IoC;

namespace Engine8.EngineCore.Resources
{

    /// <summary>
    /// IoC installer for file resource classes.
    /// </summary>
    public class ResourcesInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* IResourcePathBuilder. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IResourcePathBuilder>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton());
        }

    }

}
