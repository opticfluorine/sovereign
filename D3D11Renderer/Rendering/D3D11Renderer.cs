using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.D3D11Renderer.Rendering.Configuration;
using Sovereign.D3D11Renderer.Rendering.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sovereign.D3D11Renderer.Rendering
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

        /// <summary>
        /// Render stages.
        /// </summary>
        private readonly List<IRenderStage> renderStages = new List<IRenderStage>();

        public D3D11Renderer(MainDisplay mainDisplay, D3D11ResourceManager resourceManager,
            D3D11Device device, IList<IRenderStage> renderStages)
        {
            this.mainDisplay = mainDisplay;
            this.resourceManager = resourceManager;
            this.device = device;
            SortRenderStages(renderStages);
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

                /* Initialize render stages. */
                foreach (var stage in renderStages)
                {
                    stage.Initialize();
                }
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Could not initialize the renderer.", e);
            }
        }

        public void Cleanup()
        {
            /* Release render stages. */
            foreach (var stage in renderStages)
            {
                stage.Cleanup();
            }

            /* Release resources. */
            resourceManager.Cleanup();

            /* Release device. */
            device.Cleanup();
        }

        public void Render()
        {
            /* Perform rendering. */
            foreach (var stage in renderStages)
            {
                stage.Render();
            }

            /* Present the next frame. */
            device.Present();
        }

        /// <summary>
        /// Sorts the render stages into the order in which they are executed.
        /// </summary>
        /// <param name="stages">Render stages.</param>
        private void SortRenderStages(IList<IRenderStage> stages)
        {
            renderStages.AddRange(stages.OrderBy(stage => stage.Priority));
        }

    }

}
