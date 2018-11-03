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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="texCoordBuffer">Texture coordinate buffer for update.</param>
        /// <param name="drawLengths">Number of vertices to use for each sequential draw.</param>
        /// <param name="drawCount">Number of draws to perform.</param>
        void PopulateBuffers(Vector3[] vertexBuffer, Vector2[] texCoordBuffer, 
            int[] drawLengths, out int drawCount);

    }

}
