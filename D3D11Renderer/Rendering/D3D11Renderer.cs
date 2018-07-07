using Engine8.ClientCore.Rendering;
using Engine8.ClientCore.Rendering.Configuration;
using Engine8.ClientCore.Rendering.Display;
using Engine8.D3D11Renderer.Rendering.Configuration;
using Engine8.D3D11Renderer.Rendering.Resources;
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
        private readonly MainDisplay mainDisplay;

        /// <summary>
        /// Device and swapchain that will be used for rendering.
        /// </summary>
        private readonly D3D11Device device;

        /// <summary>
        /// D3D11 resource manager.
        /// </summary>
        private readonly D3D11ResourceManager resourceManager;

        public D3D11Renderer(MainDisplay mainDisplay, D3D11ResourceManager resourceManager,
            D3D11Device device)
        {
            this.mainDisplay = mainDisplay;
            this.resourceManager = resourceManager;
            this.device = device;
        }

        /// <summary>
        /// Initializes the Direct3D 11-based renderer.
        /// </summary>
        /// <param name="videoAdapter">Selected video adapter.</param>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs during initialization.
        /// </exception>
        public void Initialize(IVideoAdapter videoAdapter)
        {
            try
            {
                /* Attempt to create the rendering device. */
                device.CreateDevice((D3D11VideoAdapter)videoAdapter);

                /* Install main resources. */
                resourceManager.InitializeBaseResources();
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Could not initialize the renderer.", e);
            }
        }

        public void Cleanup()
        {
            resourceManager.Cleanup();
            device.Cleanup();
        }

        public void Render()
        {
            /* Perform rendering. */

            /* Present the next frame. */
            device.Present();
        }

    }

}
