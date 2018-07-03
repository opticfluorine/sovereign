using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering
{

    /// <summary>
    /// Single stage of the rendering process.
    /// </summary>
    public interface IRenderStage
    {

        /// <summary>
        /// Executes the render stage.
        /// </summary>
        void Render();

    }

}
