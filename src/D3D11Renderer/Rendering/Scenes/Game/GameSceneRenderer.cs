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
