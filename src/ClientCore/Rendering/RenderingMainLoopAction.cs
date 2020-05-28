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
    /// as fast as possible.
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
