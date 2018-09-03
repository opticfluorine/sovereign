using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.World.Material
{

    /// <summary>
    /// IoC installer for materials support classes.
    /// </summary>
    public class EngineMaterialInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* MaterialManager. */
            container.Register(Component.For<MaterialManager>()
                .LifestyleSingleton());
        }

    }
}
