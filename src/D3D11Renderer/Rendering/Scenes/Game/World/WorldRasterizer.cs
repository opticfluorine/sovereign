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
