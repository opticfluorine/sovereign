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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Manages the vertex shader stage for the game scene.
    /// </summary>
    public sealed class WorldVertexShader : IDisposable
    {

        private readonly D3D11Device device;

        private readonly GameSceneShaders gameSceneShaders;

        private readonly GameResourceManager resourceManager;

        /// <summary>
        /// D3D11 vertex shader object.
        /// </summary>
        private VertexShader vertexShader;

        public WorldVertexShader(D3D11Device device, GameSceneShaders gameSceneShaders,
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
        public void Bind(DeviceContext context)
        {
            context.VertexShader.SetShader(vertexShader, null, 0);
            context.VertexShader.SetConstantBuffer(0, resourceManager.VertexConstantBuffer.GpuBuffer);
        }

        /// <summary>
        /// Unbinds the resources from the vertex shader.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Unbind(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetShader(null, null, 0);
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
