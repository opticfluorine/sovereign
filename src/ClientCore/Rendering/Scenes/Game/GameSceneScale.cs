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

using Sovereign.ClientCore.Rendering.Configuration;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game
{

    /// <summary>
    /// Computes the rendering scale for game rendering.
    /// </summary>
    public sealed class GameSceneScale
    {

        /// <summary>
        /// Scaling constants.
        /// </summary>
        public Vector3 Scale { get; private set; }

        public GameSceneScale(DisplayViewport viewport, IDisplayMode mode)
        {
            Scale = new Vector3()
            {
                X = (float)mode.Width / viewport.Width,
                Y = (float)mode.Height / viewport.Height,
                Z = (float)mode.Height / viewport.Height
            };
        }

        /// <summary>
        /// Applies the game scene scaling constants.
        /// </summary>
        /// <param name="vertexConstants">Vertex shader constants.</param>
        public void Apply(ref GameSceneVertexConstants vertexConstants)
        {
            vertexConstants.WorldScaleX = Scale.X;
            vertexConstants.WorldScaleY = Scale.Y;
            vertexConstants.WorldScaleZ = Scale.Z;
        }

    }

}
