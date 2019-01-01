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
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Manages the rasterizer for world rendering.
    /// </summary>
    public sealed class WorldRasterizer
    {

        private readonly D3D11Device device;
        private readonly DisplayViewport displayViewport;
        private RasterizerState rasterizerState;

        public WorldRasterizer(D3D11Device device, DisplayViewport displayViewport)
        {
            this.device = device;
            this.displayViewport = displayViewport;
        }

        /// <summary>
        /// Initializes the rasterizer.
        /// </summary>
        public void Initialize()
        {
            var desc = new RasterizerStateDescription()
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = false,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            };
            rasterizerState = new RasterizerState(device.Device, desc);
        }

        /// <summary>
        /// Binds the rasterizer to the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Bind(DeviceContext context)
        {
            context.Rasterizer.State = rasterizerState;
            context.Rasterizer.SetViewport(0.0f, 0.0f, 
                device.DisplayMode.Width, device.DisplayMode.Height);
        }

        /// <summary>
        /// Unbinds the rasterizer from the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Unbind(DeviceContext context)
        {
        }

    }

}
