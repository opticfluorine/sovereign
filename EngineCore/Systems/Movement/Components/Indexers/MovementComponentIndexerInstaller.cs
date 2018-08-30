using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.EngineCore.Systems.Movement.Components.Indexers
{
    
    /// <summary>
    /// IoC installer for Movement system ComponentIndexers.
    /// </summary>
    public class MovementComponentIndexerInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* PositionComponentIndexer. */
            container.Register(Component.For<PositionComponentIndexer>()
                .LifestyleSingleton());
        }

    }

}
