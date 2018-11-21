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


using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game
{

    /// <summary>
    /// Responsible for configuring the camera for game scene rendering.
    /// </summary>
    public sealed class GameSceneCamera
    {

        private readonly CameraManager cameraManager;

        public GameSceneCamera(CameraManager cameraManager)
        {
            this.cameraManager = cameraManager;
        }

        /// <summary>
        /// Updates the vertex shader constant buffer to contain the
        /// correct camera coordinates.
        /// </summary>
        /// <param name="vertexConstants">Vertex shader constants.</param>
        /// <param name="timeSinceTick">Time elapsed from the start of the current tick.</param>
        public void Aim(ref GameSceneVertexConstants vertexConstants, float timeSinceTick)
        {
            var pos = cameraManager.Position;
            var vel = cameraManager.Velocity;

            var adjPos = pos.InterpolateByTime(vel, timeSinceTick);

            vertexConstants.CameraX = adjPos.X;
            vertexConstants.CameraY = adjPos.Y;
            vertexConstants.CameraZ = adjPos.Z;
        }

    }

}
