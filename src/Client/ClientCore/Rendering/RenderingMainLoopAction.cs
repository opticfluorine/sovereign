﻿/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Main loop action that attempts to execute the renderer on the main thread
///     as fast as possible (up to an optional maximum framerate).
/// </summary>
public class RenderingMainLoopAction : IMainLoopAction
{
    private readonly ClientStateController clientStateController;
    private readonly ClientStateServices clientStateServices;
    private readonly IEventSender eventSender;
    private readonly ILogger<RenderingMainLoopAction> logger;

    /// <summary>
    ///     Minimum system time delta between frames.
    /// </summary>
    private readonly ulong minimumTimeDelta;

    private readonly RenderingManager renderingManager;
    private readonly Stopwatch stopwatch = new();
    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     System time of the last frame.
    /// </summary>
    private ulong lastFrameTime;

    public RenderingMainLoopAction(RenderingManager renderingManager,
        ISystemTimer systemTimer, IOptions<DisplayOptions> displayOptions,
        ILogger<RenderingMainLoopAction> logger, ClientStateServices clientStateServices,
        ClientStateController clientStateController, IEventSender eventSender)
    {
        this.renderingManager = renderingManager;
        this.systemTimer = systemTimer;
        this.logger = logger;
        this.clientStateServices = clientStateServices;
        this.clientStateController = clientStateController;
        this.eventSender = eventSender;

        minimumTimeDelta = Units.SystemTime.Second / (ulong)displayOptions.Value.MaxFramerate;
    }

    // Rendering is expensive, so pump the loop multiple times between frames.
    public ulong CycleInterval => 4;

    public void Execute()
    {
        /* Ensure that the frame rate is capped appropriately. */
        var currentTime = systemTimer.GetTime();
        var delta = currentTime - lastFrameTime;
        if (delta >= minimumTimeDelta)
        {
            var debugFrame = clientStateServices.GetStateFlagValue(ClientStateFlag.DebugFrame);
            if (debugFrame)
            {
                logger.LogDebug("Begin debug frame.");
                stopwatch.Restart();
            }

            try
            {
                renderingManager.Render();
            }
            catch (FatalErrorException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception thrown during rendering.");
            }

            lastFrameTime = currentTime;
            if (debugFrame)
            {
                stopwatch.Stop();
                logger.LogDebug("End debug frame ({Time:F4} ms).", stopwatch.Elapsed.TotalMilliseconds);
                clientStateController.SetStateFlag(eventSender, ClientStateFlag.DebugFrame, false);
            }
        }
    }
}