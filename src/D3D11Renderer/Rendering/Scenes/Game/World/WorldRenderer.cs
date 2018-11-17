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
        /// <param name="vertexOffset">First vertex in the buffer to use.</param>
        /// <param name="vertexCount">Number of vertices in the buffer to draw.</param>
        private void DrawLayer(DeviceContext context, int vertexOffset, int vertexCount)
        {
            context.Draw(vertexCount, vertexOffset);
        }

    }

}
