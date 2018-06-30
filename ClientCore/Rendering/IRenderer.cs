using Engine8.ClientCore.Rendering.Configuration;
using Engine8.ClientCore.Rendering.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Implemented by renderers.
    /// </summary>
    public interface IRenderer
    {

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="mainDisplay">Main display to be backed by this renderer.</param>
        /// <param name="videoAdapter">Video adapter to use.</param>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs while initializing the renderer.
        /// </exception>
        void Initialize(MainDisplay mainDisplay, IVideoAdapter videoAdapter);

        /// <summary>
        /// Shuts down and cleans up the renderer.
        /// </summary>
        void Cleanup();

    }

}
