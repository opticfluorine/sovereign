using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

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

            /* SpriteSheetManager. */
            container.Register(Component.For<SpriteSheetManager>()
                .LifestyleSingleton());

            /* SpriteSheetDefinitionLoader. */
            container.Register(Component.For<SpriteSheetDefinitionLoader>()
                .LifestyleSingleton());
        }

    }

}
