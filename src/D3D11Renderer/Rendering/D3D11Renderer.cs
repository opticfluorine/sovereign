/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using SharpDX.Mathematics.Interop;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.D3D11Renderer.Rendering.Configuration;
using Sovereign.D3D11Renderer.Rendering.Resources;
using Sovereign.D3D11Renderer.Rendering.Scenes.Game;
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
        /// Color to clear the screen before rendering.
        /// </summary>
        public readonly RawColor4 ClearColor = new RawColor4()
        {
            R = 0.0f,
            G = 0.0f,
            B = 0.0f,
            A = 0.0f
        };

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
        /// Scene consumer.
        /// </summary>
        private readonly D3D11SceneConsumer sceneConsumer;

        /// <summary>
        /// Scene manager.
        /// </summary>
        private readonly SceneManager sceneManager;

        private readonly GameSceneRenderer gameSceneRenderer;

        public D3D11Renderer(MainDisplay mainDisplay, D3D11ResourceManager resourceManager,
            D3D11Device device, D3D11SceneConsumer sceneConsumer, SceneManager sceneManager,
            GameSceneRenderer gameSceneRenderer)
        {
            this.mainDisplay = mainDisplay;
            this.resourceManager = resourceManager;
            this.device = device;
            this.sceneConsumer = sceneConsumer;
            this.sceneManager = sceneManager;
            this.gameSceneRenderer = gameSceneRenderer;
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

                /* Initialize all scenes. */
                gameSceneRenderer.Initialize();
            }
            catch (Exception e)
            {
                throw new RendererInitializationException("Could not initialize the renderer.", e);
            }
        }

        public void Cleanup()
        {
            /* Clean up all scenes. */
            gameSceneRenderer.Dispose();

            /* Release resources. */
            resourceManager.Cleanup();

            /* Release device. */
            device.Cleanup();
        }

        public void Render()
        {
            /* Prepare for rendering. */
            ClearScreen();

            /* Hand the current scene off to the top-level consumer. */
            var scene = sceneManager.ActiveScene;
            scene.BeginScene();
            sceneConsumer.ConsumeScene(scene);
            scene.EndScene();

            /* Present the next frame. */
            device.Present();
        }
        
        /// <summary>
        /// Clears the screen.
        /// </summary>
        private void ClearScreen()
        {
            var context = device.Device.ImmediateContext;
            context.ClearRenderTargetView(device.BackBufferView, ClearColor);
        }

    }

}
