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
    /// Responsible for drawing single layers of the world.
    /// </summary>
    public sealed class WorldRenderer 
    {
        private readonly WorldInputAssembler inputAssembler;
        private readonly WorldVertexShader vertexShader;
        private readonly WorldRasterizer rasterizer;
        private readonly WorldPixelShader pixelShader;
        private readonly WorldOutputMerger outputMerger;

        public WorldRenderer(WorldInputAssembler inputAssembler, WorldVertexShader vertexShader,
            WorldRasterizer rasterizer, WorldPixelShader pixelShader, WorldOutputMerger outputMerger)
        {
            this.inputAssembler = inputAssembler;
            this.vertexShader = vertexShader;
            this.rasterizer = rasterizer;
            this.pixelShader = pixelShader;
            this.outputMerger = outputMerger;
        }

        /// <summary>
        /// Initializes the world renderer and its components.
        /// </summary>
        public void Initialize()
        {
            inputAssembler.Initialize();
            vertexShader.Initialize();
            rasterizer.Initialize();
            pixelShader.Initialize();
            outputMerger.Initialize();
        }

        /// <summary>
        /// Renders a single layer of the world.
        /// </summary>
        /// <param name="context">Device context.</param>
        /// <param name="vertexOffset">First vertex in the buffer to use.</param>
        /// <param name="vertexCount">Number of vertices in the buffer to draw.</param>
        public void RenderLayer(DeviceContext context, int vertexOffset, int vertexCount)
        {
            BindPipeline(context);
            DrawLayer(context, vertexOffset, vertexCount);
            UnbindPipeline(context);
        }

        /// <summary>
        /// Binds all resources to the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void BindPipeline(DeviceContext context)
        {
            inputAssembler.Bind(context);
            vertexShader.Bind(context);
            rasterizer.Bind(context);
            pixelShader.Bind(context);
            outputMerger.Bind(context);
        }

        /// <summary>
        /// Unbinds all resources from the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void UnbindPipeline(DeviceContext context)
        {
            pixelShader.Unbind(context);
            rasterizer.Unbind(context);
            vertexShader.Unbind(context);
            inputAssembler.Unbind(context);
            outputMerger.Unbind(context);
        }

        /// <summary>
        /// Draws a single layer of the world.
        /// </summary>
        /// <param name="context">Device context.</param>
        /// <param name="indexOffset">First index to use in the index buffer.</param>
        /// <param name="indexCount">Number of indices to use from the index buffer.</param>
        private void DrawLayer(DeviceContext context, int indexOffset, int indexCount)
        {
            context.DrawIndexed(indexCount, indexOffset, 0);
        }

    }

}
