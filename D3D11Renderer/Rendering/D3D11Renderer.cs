using Engine8.ClientCore.Rendering;
using Engine8.ClientCore.Rendering.Configuration;
using Engine8.ClientCore.Rendering.Display;
using Engine8.D3D11Renderer.Rendering.Configuration;
using System;

namespace Engine8.D3D11Renderer.Rendering
{

    /// <summary>
    /// Renderer implementation using Direct3D 11.
    /// </summary>
    public class D3D11Renderer : IRenderer
    {

        /// <summary>
        /// Device and swapchain that will be used for rendering.
        /// </summary>
        private D3D11Device device;

        public void Initialize(MainDisplay mainDisplay, IVideoAdapter videoAdapter)
        {
            /* Attempt to create the rendering device. */
            try
            {
                device = new D3D11Device(mainDisplay, (D3D11VideoAdapter)videoAdapter);
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Failed to create the Direct3D 11 device.", e);
            }
        }

        public void Cleanup()
        {

        }

    }

}
