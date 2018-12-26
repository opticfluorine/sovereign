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
            mat.M12 = 0.0f;
            mat.M13 = 0.0f;
            mat.M14 = -invHalfWidth * cameraPos.X;

            mat.M21 = 0.0f;
            mat.M22 = invHalfHeight;
            mat.M23 = -invHalfHeight;
            mat.M24 = invHalfHeight * (cameraPos.Z - cameraPos.Y);

            mat.M31 = 0.0f;
            mat.M32 = 0.0f;
            mat.M33 = 0.0f;
            mat.M34 = -1.0f;

            mat.M41 = 0.0f;
            mat.M42 = 0.0f;
            mat.M43 = 0.0f;
            mat.M44 = 1.0f;
        }

    }

}
