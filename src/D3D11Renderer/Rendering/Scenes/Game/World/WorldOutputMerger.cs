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
    /// Manages the output-merger stage for world rendering.
    /// </summary>
    public sealed class WorldOutputMerger
    {

        private readonly D3D11Device device;

        /// <summary>
        /// Blend state.
        /// </summary>
        private BlendState blendState;

        public WorldOutputMerger(D3D11Device device)
        {
            this.device = device;
        }

        /// <summary>
        /// Initializes the output-merger stage.
        /// </summary>
        public void Initialize()
        {
            var desc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            desc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };

            blendState = new BlendState(device.Device, desc);
        }

        /// <summary>
        /// Binds the output-merger stage to the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Bind(DeviceContext context)
        {
            context.OutputMerger.SetRenderTargets(device.BackBufferView);
            context.OutputMerger.SetBlendState(blendState);
        }

        /// <summary>
        /// Unbinds the output-merger stage from the pipeline.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Unbind(DeviceContext context)
        {
            context.OutputMerger.SetBlendState(null);
            context.OutputMerger.SetRenderTargets((RenderTargetView)null);
        }

    }

}
