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

using SharpDX.Mathematics.Interop;
using Sovereign.ClientCore.Rendering.Scenes;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Responsible for updating the world rendering vertex constant buffers.
    /// </summary>
    public sealed class WorldVertexConstantsUpdater
    {
        private readonly GameResourceManager gameResourceManager;

        public WorldVertexConstantsUpdater(GameResourceManager gameResourceManager)
        {
            this.gameResourceManager = gameResourceManager;
        }

        /// <summary>
        /// Updates the constant buffers for the vertex shader for world rendering.
        /// </summary>
        /// <param name="scene">Active scene.</param>
        public void Update(IScene scene)
        {
            /* Retrieve the needed constants. */
            scene.PopulateWorldVertexConstants(out var widthInTiles,
                out var heightInTiles,
                out var cameraPos,
                out var timeSinceTick);
            var invHalfWidth = 2.0f / widthInTiles;
            var invHalfHeight = 2.0f / heightInTiles;

            /* Update constant buffer. */
            var buf = gameResourceManager.VertexConstantBuffer.Buffer;
            buf[0].TimeSinceTick = timeSinceTick;

            /* Calculate world-view transform matrix. */
            ref var mat = ref buf[0].WorldViewTransform;

            mat.M11 = invHalfWidth;
            mat.M21 = 0.0f;
            mat.M31 = 0.0f;
            mat.M41 = -invHalfWidth * cameraPos.X;

            mat.M12 = 0.0f;
            mat.M22 = invHalfHeight;
            mat.M32 = -invHalfHeight;
            mat.M42 = invHalfHeight * (cameraPos.Z - cameraPos.Y);

            mat.M13 = 0.0f;
            mat.M23 = 0.0f;
            mat.M33 = 0.0f;
            mat.M43 = 0.0f;

            mat.M14 = 0.0f;
            mat.M24 = 0.0f;
            mat.M34 = 0.0f;
            mat.M44 = 1.0f;
        }

    }

}
