using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering
{

    /// <summary>
    /// IoC installer for the D3D11 renderer.
    /// </summary>
    public class D3D11RenderingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* D3D11Device. */
            container.Register(Component.For<D3D11Device>()
                .LifestyleSingleton());

            /* IRenderStage. */
            container.Register(Classes.FromThisAssembly()
                .BasedOn<IRenderStage>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                .AllowMultipleMatches());
        }
    }
}
