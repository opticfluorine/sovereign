using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering.World
{

    /// <summary>
    /// Responsible for rendering the game world with associated state.
    /// </summary>
    public class D3D11WorldRenderStage : IRenderStage
    {

        /// <summary>
        /// Rendering device.
        /// </summary>
        private D3D11Device device;

        public D3D11WorldRenderStage(D3D11Device device)
        {
            this.device = device;
        }

        public void Render()
        {

        }

    }

}
