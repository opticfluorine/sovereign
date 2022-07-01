/*
 * Sovereign Engine
 * Copyright (c) 2021 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering
{

    /// <summary>
    /// Renderer implementation using the Veldrid graphics library.
    /// </summary>
    public class VeldridRenderer : IRenderer
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Veldrid graphics device.
        /// </summary>
        private readonly VeldridDevice device;

        /// <summary>
        /// Veldrid resource manager.
        /// </summary>
        private readonly VeldridResourceManager resourceManager;

        /// <summary>
        /// Scene manager.
        /// </summary>
        private readonly SceneManager sceneManager;

        /// <summary>
        /// Scene consumer.
        /// </summary>
        private readonly VeldridSceneConsumer sceneConsumer;

        /// <summary>
        /// Dispose flag.
        /// </summary>
        private bool isDisposed = false;

        public VeldridRenderer(VeldridDevice device, VeldridResourceManager resourceManager,
            SceneManager sceneManager, VeldridSceneConsumer sceneConsumer)
        {
            this.device = device;
            this.resourceManager = resourceManager;
            this.sceneManager = sceneManager;
            this.sceneConsumer = sceneConsumer;
        }

        public void Initialize(IVideoAdapter videoAdapter)
        {
            try
            {
                /* Attempt to create the rendering device. */
                device.CreateDevice();

                /* Install main resources. */
                resourceManager.InitializeBaseResources();

                /* Initialize all scenes. */
                sceneConsumer.Initialize();
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Failed to initialize renderer.", e);
            }
        }

        public void Cleanup()
        {
            if (!isDisposed)
            {
                sceneConsumer.Dispose();
                resourceManager.Dispose();
                device.Dispose();
                isDisposed = true;
            }
        }

        public void Render()
        {
            try
            {
                /* Prepare for rendering. */
                var commandList = resourceManager.CommandList;
                commandList.Begin();
                commandList.SetFramebuffer(device.Device.SwapchainFramebuffer);
                commandList.ClearColorTarget(0, RgbaFloat.Black);

                /* Do scene processing to fill buffers for GPU. */
                var scene = sceneManager.ActiveScene;
                scene.BeginScene();
                sceneConsumer.ConsumeScene(scene);
                scene.EndScene();

                /* Render and present the next frame. */
                commandList.End();
                device.Device.SubmitCommands(commandList);
                device.Device.WaitForIdle();
                device.Device.SwapBuffers();
            }
            catch (Exception e)
            {
                Logger.Error("Error during rendering.", e);
            }
        }

    }
}