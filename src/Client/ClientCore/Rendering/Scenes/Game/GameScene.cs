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

using System.Numerics;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game
{

    /// <summary>
    /// In-game scene.
    /// </summary>
    public sealed class GameScene : IScene
    {
        private readonly ISystemTimer systemTimer;
        private readonly IEngineConfiguration engineConfiguration;
        private readonly GameSceneCamera camera;
        private readonly DisplayViewport viewport;
        private readonly MainDisplay mainDisplay;
        private readonly WorldVertexSequencer worldVertexSequencer;

        /// <summary>
        /// System time of the current frame, in microseconds.
        /// </summary>
        private ulong systemTime;

        /// <summary>
        /// Time since the current tick started, in seconds.
        /// </summary>
        /// Evaluated at the start of rendering.
        private float timeSinceTick;

        public SceneType SceneType => SceneType.Game;

        public bool RenderGui => true;

        public GameScene(ISystemTimer systemTimer, IEngineConfiguration engineConfiguration,
            GameSceneCamera camera, DisplayViewport viewport, MainDisplay mainDisplay,
            WorldVertexSequencer worldVertexSequencer)
        {
            this.systemTimer = systemTimer;
            this.engineConfiguration = engineConfiguration;
            this.camera = camera;
            this.viewport = viewport;
            this.mainDisplay = mainDisplay;
            this.worldVertexSequencer = worldVertexSequencer;
        }

        public void BeginScene()
        {
            ComputeTimes();
        }

        public void EndScene()
        {

        }

        public void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
            int[] drawLengths, out int drawCount)
        {
            worldVertexSequencer.SequenceVertices(vertexBuffer, indexBuffer, drawLengths,
                out drawCount, timeSinceTick, systemTime);
        }

        public void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles,
            out Vector3 cameraPos, out float timeSinceTick)
        {
            widthInTiles = viewport.WidthInTiles;
            heightInTiles = viewport.HeightInTiles;
            cameraPos = camera.Aim(this.timeSinceTick);
            timeSinceTick = this.timeSinceTick;
        }

        public void PopulateGuiBuffers(ImDrawVert[] vertexBuffer, ushort[] indexBuffer)
        {
            // TODO
        }

        /// <summary>
        /// Updates systemTime and timeSinceTick.
        /// </summary>
        private void ComputeTimes()
        {
            systemTime = systemTimer.GetTime();
            timeSinceTick = (systemTime % engineConfiguration.EventTickInterval)
                * UnitConversions.UsToS;
        }

    }

}
