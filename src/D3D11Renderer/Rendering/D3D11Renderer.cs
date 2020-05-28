/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
