using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Implemented by renderers.
    /// </summary>
    public interface IRenderer
    {

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="videoAdapter">Video adapter to use.</param>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs while initializing the renderer.
        /// </exception>
        void Initialize(IVideoAdapter videoAdapter);

        /// <summary>
        /// Shuts down and cleans up the renderer.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Renders the next frame.
        /// </summary>
        void Render();

    }

}
