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
        /// Time since the current tick started, in seconds.
        /// </summary>
        /// Evaluated at the start of rendering.
        private float timeSinceTick;

        /// <summary>
        /// Game scene scaling constants.
        /// </summary>
        private GameSceneScale scale;

        public SceneType SceneType => SceneType.Game;

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
            ComputeTimeSinceTick();
        }

        public void EndScene()
        {

        }

        public void PopulateBuffers(Pos3Tex2Vertex[] vertexBuffer, int[] indexBuffer,
            int[] drawLengths, out int drawCount)
        {
            worldVertexSequencer.SequenceVertices(vertexBuffer, drawLengths, indexBuffer,
                out drawCount, timeSinceTick);
        }

        public void PopulateGameSceneVertexConstantBuffer(GameSceneVertexConstants[] constantBuffer)
        {
            if (scale == null)
                scale = new GameSceneScale(viewport, mainDisplay.DisplayMode);

            scale.Apply(ref constantBuffer[0]);
            camera.Aim(ref constantBuffer[0], timeSinceTick);
        }

        /// <summary>
        /// Updates timeSinceTick.
        /// </summary>
        private void ComputeTimeSinceTick()
        {
            var systemTime = systemTimer.GetTime();
            timeSinceTick = (systemTime % engineConfiguration.EventTickInterval)
                * UnitConversions.UsToS;
        }

    }

}
