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

using Castle.Core.Logging;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Main loop action that attempts to execute the renderer on the main thread
    /// as fast as possible (up to an optional maximum framerate).
    /// </summary>
    public class RenderingMainLoopAction : IMainLoopAction
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private readonly RenderingManager renderingManager;
        private readonly ISystemTimer systemTimer;

        // Rendering is expensive, so pump the loop multiple times between frames.
        public ulong CycleInterval => 4;

        /// <summary>
        /// Minimum system time delta between frames.
        /// </summary>
        private readonly ulong minimumTimeDelta;

        /// <summary>
        /// System time of the last frame.
        /// </summary>
        private ulong lastFrameTime = 0;

        public RenderingMainLoopAction(RenderingManager renderingManager,
            ISystemTimer systemTimer, IClientConfiguration clientConfiguration)
        {
            this.renderingManager = renderingManager;
            this.systemTimer = systemTimer;

            minimumTimeDelta = Units.SystemTime.Second / (ulong)clientConfiguration.MaxFramerate;
        }

        public void Execute()
        {
            /* Ensure that the frame rate is capped appropriately. */
            var currentTime = systemTimer.GetTime();
            var delta = currentTime - lastFrameTime;
            if (delta >= minimumTimeDelta)
            {
                renderingManager.Render();
                lastFrameTime = currentTime;
            }
        }

    }

}
