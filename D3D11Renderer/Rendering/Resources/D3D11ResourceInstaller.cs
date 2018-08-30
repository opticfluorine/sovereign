using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.D3D11Renderer.Rendering.Resources
{

    /// <summary>
    /// IoC installer for D3D11 renderer resources.
    /// </summary>
    public class D3D11ResourceInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* D3D11ResourceManager. */
            container.Register(Component.For<D3D11ResourceManager>()
                .LifestyleSingleton());
        }

    }
}
