using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// IoC installer for tile sprites.
    /// </summary>
    public sealed class TileSpriteInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* TileSpriteDefinitionsLoader. */
            container.Register(Component.For<TileSpriteDefinitionsLoader>()
                .LifestyleSingleton());

            /* TileSpriteDefinitionsValidator. */
            container.Register(Component.For<TileSpriteDefinitionsValidator>()
                .LifestyleSingleton());

            /* TileSpriteManager. */
            container.Register(Component.For<TileSpriteManager>()
                .LifestyleSingleton());
        }
    }

}
