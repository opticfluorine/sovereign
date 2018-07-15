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
        /// Stage priorty. Stages with smaller priorities are executed earlier.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Initializes the render stage.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Executes the render stage.
        /// </summary>
        void Render();

        /// <summary>
        /// Cleans up the render stage.
        /// </summary>
        void Cleanup();

    }

}
