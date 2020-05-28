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

using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Scenes.Game;
using Sovereign.D3D11Renderer.Rendering.Resources.Buffers;
using Sovereign.D3D11Renderer.Rendering.Scenes.Game.World;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Manages rendering resources for the game scene.
    /// </summary>
    public sealed class GameResourceManager
    {

        /// <summary>
        /// Maximum number of draws.
        /// </summary>
        public const int MaximumDraws = 1024;

        /// <summary>
        /// Maximum number of vertices in the vertex buffer.
        /// </summary>
        public const int MaximumVertices = 16384;

        /// <summary>
        /// Maximum number of indices in the index buffer.
        /// </summary>
        public const int MaximumIndices = 24576;

        /// <summary>
        /// Updateable vertex buffer.
        /// </summary>
        public D3D11UpdateBuffer<WorldVertex> VertexBuffer { get; private set; }

        /// <summary>
        /// Index buffer into the vertex buffer.
        /// </summary>
        public D3D11UpdateBuffer<uint> IndexBuffer { get; private set; }

        /// <summary>
        /// Constant buffer for the vertex shader.
        /// </summary>
        public D3D11UpdateBuffer<WorldVertexShaderConstants> VertexConstantBuffer { get; private set; }

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
            DrawBuffer = new int[MaximumDraws];
        }

        /// <summary>
        /// Initializes the resources.
        /// </summary>
        public void Initialize()
        {
            /* Load shader bytecode. */
            shaders.Initialize();

            /* Create buffers. */
            VertexBuffer = new D3D11UpdateBuffer<WorldVertex>(device, BindFlags.VertexBuffer,
                MaximumVertices);

            IndexBuffer = new D3D11UpdateBuffer<uint>(device, BindFlags.IndexBuffer,
                MaximumIndices);

            VertexConstantBuffer = new D3D11UpdateBuffer<WorldVertexShaderConstants>(device,
                BindFlags.ConstantBuffer, 1);
        }

        /// <summary>
        /// Frees the resources.
        /// </summary>
        public void Cleanup()
        {
            VertexConstantBuffer?.Dispose();
            VertexConstantBuffer = null;

            IndexBuffer?.Dispose();
            IndexBuffer = null;

            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }

        /// <summary>
        /// Updates all resource buffers.
        /// </summary>
        public void UpdateBuffers()
        {
            VertexBuffer.Update();
            IndexBuffer.Update();
            VertexConstantBuffer.Update();
        }

    }

}
