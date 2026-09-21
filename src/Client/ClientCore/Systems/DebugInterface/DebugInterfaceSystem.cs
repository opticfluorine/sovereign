// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.DebugInterface;

/// <summary>
///     System that services the client debug interface.
/// </summary>
public class DebugInterfaceSystem : ISystem
{
    private readonly FrameCaptureQueue frameCaptureQueue;
    private readonly ILogger<DebugInterfaceSystem> logger;
    private readonly DebugInterfaceOptions options;
    private readonly DebugRequestProcessor requestProcessor;
    private readonly DebugInterfaceServer server;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>();

    public int WorkloadEstimate { get; } = 5;

    public DebugInterfaceSystem(EventCommunicator eventCommunicator,
        DebugInterfaceServer server, DebugRequestProcessor requestProcessor,
        FrameCaptureQueue frameCaptureQueue, IOptions<DebugInterfaceOptions> options,
        ILogger<DebugInterfaceSystem> logger)
    {
        EventCommunicator = eventCommunicator;
        this.server = server;
        this.requestProcessor = requestProcessor;
        this.frameCaptureQueue = frameCaptureQueue;
        this.options = options.Value;
        this.logger = logger;
    }

    public void Initialize()
    {
        if (options.Enabled)
        {
            server.Start();
        }
        else
        {
            logger.LogInformation("Debug interface is disabled.");
        }
    }

    public void Cleanup()
    {
        server.Stop();
    }

    public int ExecuteOnce()
    {
        server.Poll();
        return DrainScreenshotCompletions();
    }

    /// <summary>
    ///     Drains completed frame captures and delivers each one to the peer
    ///     that requested the corresponding screenshot.
    /// </summary>
    /// <returns>Number of completions processed.</returns>
    private int DrainScreenshotCompletions()
    {
        var processed = 0;
        while (frameCaptureQueue.TryDequeueCompletion(out var result))
        {
            processed++;
            if (!requestProcessor.TryTakePendingPeer(result.RequestId, out var peer)) continue;
            requestProcessor.EnqueueResponse(peer,
                MessageConfig.SerializeMsgPack(CreateScreenshotResponse(result)));
        }

        return processed;
    }

    /// <summary>
    ///     Builds the response for a completed screenshot request.
    /// </summary>
    /// <param name="result">Completed capture result.</param>
    /// <returns>Response to send to the requesting peer.</returns>
    private static DebugResponse CreateScreenshotResponse(FrameCaptureResult result)
    {
        if (result.Frame is not { } frame)
        {
            return new DebugResponse
            {
                RequestId = result.RequestId,
                Status = DebugResponseStatus.Error,
                ErrorMessage = "Frame capture failed."
            };
        }

        return new DebugResponse
        {
            RequestId = result.RequestId,
            Status = DebugResponseStatus.Ok,
            Details = new ScreenshotResponseDetails
            {
                Width = frame.Width,
                Height = frame.Height,
                Format = frame.Format,
                Pixels = frame.Pixels
            }
        };
    }
}
