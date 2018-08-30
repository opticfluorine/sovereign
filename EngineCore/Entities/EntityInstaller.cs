using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.EngineCore.Entities
{

    /// <summary>
    /// IoC installer for the entity infrastructure.
    /// </summary>
    public class EntityInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* EntityManager. */
            container.Register(Component.For<EntityManager>()
                .LifestyleSingleton());
        }

    }

}
