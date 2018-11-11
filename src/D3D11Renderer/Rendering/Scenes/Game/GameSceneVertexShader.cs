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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Manages the vertex shader stage for the game scene.
    /// </summary>
    public sealed class GameSceneVertexShader : IDisposable
    {

        private readonly D3D11Device device;

        private readonly GameSceneShaders gameSceneShaders;

        private readonly GameResourceManager resourceManager;

        /// <summary>
        /// D3D11 vertex shader object.
        /// </summary>
        private VertexShader vertexShader;

        public GameSceneVertexShader(D3D11Device device, GameSceneShaders gameSceneShaders,
            GameResourceManager resourceManager)
        {
            this.device = device;
            this.gameSceneShaders = gameSceneShaders;
            this.resourceManager = resourceManager;
        }

        /// <summary>
        /// Initializes the vertex shader.
        /// </summary>
        public void Initialize()
        {
            vertexShader = CreateShader();
        }

        public void Dispose()
        {
            vertexShader?.Dispose();
        }

        /// <summary>
        /// Configures the vertex shader stage for the given context.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Configure(DeviceContext context)
        {
            context.VertexShader.SetShader(vertexShader, null, 0);
            context.VertexShader.SetConstantBuffer(0, resourceManager.VertexConstantBuffer.GpuBuffer);
        }

        /// <summary>
        /// Creates the D3D11 vertex shader object.
        /// </summary>
        /// <returns>Vertex shader object.</returns>
        private VertexShader CreateShader()
        {
            return new VertexShader(device.Device, gameSceneShaders.WorldVertexShader);
        }

    }

}
