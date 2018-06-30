using Engine8.ClientCore.Rendering.Configuration;
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
        /// <param name="windowHandle">Main window handle.</param>
        /// <param name="videoAdapter">Video adapter to use.</param>
        void Initialize(IntPtr windowHandle, IVideoAdapter videoAdapter);

        /// <summary>
        /// Shuts down and cleans up the renderer.
        /// </summary>
        void Cleanup();

    }

}
