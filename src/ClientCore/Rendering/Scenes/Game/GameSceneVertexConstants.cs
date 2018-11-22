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

using System.Runtime.InteropServices;

namespace Sovereign.ClientCore.Rendering.Scenes.Game
{

    /// <summary>
    /// Constant buffer structure used by the game scene vertex shader.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct GameSceneVertexConstants
    {

        /// <summary>
        /// Conversion from 1.0 x position units to pixels.
        /// </summary>
        [FieldOffset(0)] public float WorldScaleX;

        /// <summary>
        /// Conversion from 1.0 y position units to pixels.
        /// </summary>
        [FieldOffset(sizeof(float))] public float WorldScaleY;

        /// <summary>
        /// Conversion from 1.0 z position units to pixels.
        /// </summary>
        [FieldOffset(2 * sizeof(float))] public float WorldScaleZ;

        /// <summary>
        /// Camera position along x.
        /// </summary>
        [FieldOffset(3 * sizeof(float))] public float CameraX;

        /// <summary>
        /// Camera position along y.
        /// </summary>
        [FieldOffset(4 * sizeof(float))] public float CameraY;

        /// <summary>
        /// Camera position along z.
        /// </summary>
        [FieldOffset(5 * sizeof(float))] public float CameraZ;

        /// <summary>
        /// Width of the display in pixels.
        /// </summary>
        [FieldOffset(6 * sizeof(float))] public float DisplayWidth;

        /// <summary>
        /// Height of the display in pixels.
        /// </summary>
        [FieldOffset(7 * sizeof(float))] public float DisplayHeight;

    }

}
