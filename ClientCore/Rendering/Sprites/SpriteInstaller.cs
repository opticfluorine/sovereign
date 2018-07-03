using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// IoC installer for sprite-related services.
    /// </summary>
    public class SpriteInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* SurfaceLoader. */
            container.Register(Component.For<SurfaceLoader>()
                .LifestyleSingleton());

            /* SpriteSheetFactory. */
            container.Register(Component.For<SpriteSheetFactory>()
                .LifestyleSingleton());
        }

    }

}
