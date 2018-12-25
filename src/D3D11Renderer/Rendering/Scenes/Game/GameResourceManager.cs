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
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Scenes.Game;
using Sovereign.D3D11Renderer.Rendering.Resources.Buffers;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Manages rendering resources for the game scene.
    /// </summary>
    public sealed class GameResourceManager
    {

        /// <summary>
        /// Maximum number of elements in a main resource buffer.
        /// </summary>
        public const int MaximumBufferElements = 4096;

        /// <summary>
        /// Updateable vertex buffer.
        /// </summary>
        public D3D11UpdateBuffer<Pos3Tex2Vertex> VertexBuffer { get; private set; }

        /// <summary>
        /// Index buffer into the vertex buffer.
        /// </summary>
        public D3D11UpdateBuffer<int> IndexBuffer { get; private set; }

        /// <summary>
        /// Constant buffer for the vertex shader.
        /// </summary>
        public D3D11UpdateBuffer<GameSceneVertexConstants> VertexConstantBuffer { get; private set; }

        /// <summary>
        /// Number of elements to use in each draw.
        /// </summary>
        public int[] DrawBuffer { get; private set; }

        /// <summary>
        /// Number of draws to be performed.
        /// </summary>
        public int DrawCount { get; set; }

        private readonly D3D11Device device;

        private readonly GameSceneShaders shaders;

        public GameResourceManager(D3D11Device device, GameSceneShaders shaders)
        {
            this.device = device;
            this.shaders = shaders;
            DrawBuffer = new int[MaximumBufferElements];
        }

        /// <summary>
        /// Initializes the resources.
        /// </summary>
        public void Initialize()
        {
            /* Load shader bytecode. */
            shaders.Initialize();

            /* Create buffers. */
            VertexBuffer = new D3D11UpdateBuffer<Pos3Tex2Vertex>(device, BindFlags.VertexBuffer,
                MaximumBufferElements);

            VertexConstantBuffer = new D3D11UpdateBuffer<GameSceneVertexConstants>(device,
                BindFlags.ConstantBuffer, 1);
        }

        /// <summary>
        /// Frees the resources.
        /// </summary>
        public void Cleanup()
        {
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }

        /// <summary>
        /// Updates all resource buffers.
        /// </summary>
        public void UpdateBuffers()
        {
            VertexBuffer.Update();
            VertexConstantBuffer.Update();
        }

    }

}
