using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering.World
{

    public class WorldRenderStageInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* SingleLayerRenderStep. */
            container.Register(Component.For<SingleLayerTileRenderStep>()
                .LifestyleSingleton());
        }
    }
}
