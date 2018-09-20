using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.ClientCore.Rendering.Sprites
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

            /* SpriteSheetDefinitionValidator. */
            container.Register(Component.For<SpriteSheetDefinitionValidator>()
                .LifestyleSingleton());

            /* TextureAtlasManager. */
            container.Register(Component.For<TextureAtlasManager>()
                .LifestyleSingleton());

            /* SpriteManager. */
            container.Register(Component.For<SpriteManager>()
                .LifestyleSingleton());

            /* SpriteDefinitionsLoader. */
            container.Register(Component.For<SpriteDefinitionsLoader>()
                .LifestyleSingleton());

            /* SpriteDefinitionsValidator. */
            container.Register(Component.For<SpriteDefinitionsValidator>()
                .LifestyleSingleton());
        }

    }

}
