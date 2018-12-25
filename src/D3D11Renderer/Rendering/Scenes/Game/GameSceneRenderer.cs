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

using SharpDX.Direct3D11;
using Sovereign.D3D11Renderer.Rendering.Scenes.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Responsible for rendering the game scene.
    /// </summary>
    public sealed class GameSceneRenderer : IDisposable
    {

        private readonly D3D11Device device;
        private readonly GameResourceManager gameResourceManager;
        private readonly WorldRenderer worldRenderer;

        public GameSceneRenderer(D3D11Device device, GameResourceManager gameResourceManager,
            WorldRenderer worldRenderer)
        {
            this.device = device;
            this.gameResourceManager = gameResourceManager;
            this.worldRenderer = worldRenderer;
        }

        /// <summary>
        /// Initializes the game scene renderer.
        /// </summary>
        public void Initialize()
        {
            gameResourceManager.Initialize();
            worldRenderer.Initialize();
        }

        public void Dispose()
        {
            gameResourceManager.Cleanup();
        }

        /// <summary>
        /// Renders the game scene.
        /// </summary>
        public void Render()
        {
            var context = device.Device.ImmediateContext;
            RenderWorld(context);
        }

        /// <summary>
        /// Renders the game world.
        /// </summary>
        private void RenderWorld(DeviceContext context)
        {
            /* Bind pipeline. */
            worldRenderer.BindPipeline(context);

            /* Iterate over layers to be drawn. */
            var offset = 0;
            for (int i = 0; i < gameResourceManager.DrawCount; ++i)
            {
                var length = gameResourceManager.DrawBuffer[i];
                RenderLayer(context, offset, length);
                offset += length;
            }

            /* Unbind pipeline. */
            worldRenderer.UnbindPipeline(context);
        }

        /// <summary>
        /// Renders a single layer.
        /// </summary>
        /// <param name="context">Device context.</param>
        /// <param name="offset">Index buffer offset for layer.</param>
        /// <param name="length">Number of indices in layer.</param>
        private void RenderLayer(DeviceContext context, int offset, int length)
        {
            /* Draw the world. */
            worldRenderer.RenderLayer(context, offset, length);
            
            /* TODO: Lighting and other effects. */
        }

    }

}
