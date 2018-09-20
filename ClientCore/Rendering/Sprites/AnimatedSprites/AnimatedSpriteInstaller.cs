using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// IoC installer for animated sprites.
    /// </summary>
    public sealed class AnimatedSpriteInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<AnimatedSpriteDefinitionsLoader>()
                .LifestyleSingleton());

            container.Register(Component.For<AnimatedSpriteDefinitionsValidator>()
                .LifestyleSingleton());

            container.Register(Component.For<AnimatedSpriteManager>()
                .LifestyleSingleton());
        }
    }

}
