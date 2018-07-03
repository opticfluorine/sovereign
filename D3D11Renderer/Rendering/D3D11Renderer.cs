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
        /// Main display.
        /// </summary>
        private MainDisplay mainDisplay;

        /// <summary>
        /// Device and swapchain that will be used for rendering.
        /// </summary>
        private D3D11Device device;

        /// <summary>
        /// Initializes the Direct3D 11-based renderer.
        /// </summary>
        /// <param name="mainDisplay">Main display.</param>
        /// <param name="videoAdapter">Selected video adapter.</param>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs during initialization.
        /// </exception>
        public void Initialize(MainDisplay mainDisplay, IVideoAdapter videoAdapter)
        {
            this.mainDisplay = mainDisplay;

            /* Attempt to create the rendering device. */
            CreateDevice((D3D11VideoAdapter)videoAdapter);
        }

        public void Cleanup()
        {
            device.Dispose();
            device = null;
        }

        public void Render()
        {
            /* Perform rendering. */

            /* Present the next frame. */
            device.Present();
        }

        /// <summary>
        /// Creates the Direct3D 11 device.
        /// </summary>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs.
        /// </exception>
        private void CreateDevice(D3D11VideoAdapter videoAdapter)
        {
            try
            {
                device = new D3D11Device(mainDisplay, videoAdapter);
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Failed to create the Direct3D 11 device.", e);
            }
        }

    }

}
