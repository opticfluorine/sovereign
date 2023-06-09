﻿/*
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

using Sovereign.ClientCore.Rendering.Resources.Buffers;
using System.Numerics;
using ImGuiNET;

namespace Sovereign.ClientCore.Rendering.Scenes
{

    /// <summary>
    /// Describes a renderable scene.
    /// </summary>
    public interface IScene
    {

        /// <summary>
        /// Type of scene. This determines how the renderer processes the scene.
        /// </summary>
        SceneType SceneType { get; }

        /// <summary>
        /// Flag indicating whether the GUI should be rendered in this scene.
        /// </summary>
        bool RenderGui { get; }

        /// <summary>
        /// Called when the renderer starts rendering this scene.
        /// </summary>
        void BeginScene();

        /// <summary>
        /// Called when the renderer finishes rendering this scene.
        /// </summary>
        void EndScene();

        /// <summary>
        /// Populates the buffers used by the renderer.
        /// </summary>
        ///
        /// The low-level renderer will call this method on the active scene when it is
        /// time to update the buffers. To update the buffers, simply update the supplied
        /// arrays. These local buffers will be copied to the GPU buffers after this method
        /// returns.
        /// 
        /// The maximum number of elements in each buffer is defined by the low-level renderer
        /// and may be determined from the length of the arrays.
        ///
        /// <param name="vertexBuffer">Vertex buffer for update.</param>
        /// <param name="indexBuffer">Index buffer for update.</param>
        /// <param name="drawLengths">Number of vertices to use for each sequential draw.</param>
        /// <param name="drawCount">Number of draws to perform.</param>
        void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
            int[] drawLengths, out int drawCount);

        /// <summary>
        /// Populates the world rendering vertex constants buffer, if applicable.
        /// </summary>
        /// <param name="widthInTiles">Width of the display in tiles.</param>
        /// <param name="heightInTiles">Height of the display in tiles.</param>
        /// <param name="cameraPos">Camera position.</param>
        /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
        void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles,
            out Vector3 cameraPos, out float timeSinceTick);

        /// <summary>
        /// Populates the various buffers used for GUI rendering.
        /// </summary>
        /// <param name="vertexBuffer">Vertex buffer to be populated.</param>
        /// <param name="indexBuffer">Index buffer to be populated.</param>
        /// <remarks>
        /// This method will only be called if RenderGui is true.
        /// </remarks>
        void PopulateGuiBuffers(ImDrawVert[] vertexBuffer, ushort[] indexBuffer);

    }

}
