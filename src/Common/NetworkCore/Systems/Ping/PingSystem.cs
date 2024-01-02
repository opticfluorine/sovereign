// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;

namespace Sovereign.NetworkCore.Systems.Ping;

/// <summary>
///     System that provides ping and response services for the network. This is used both for
///     performance measurement and event server connection heartbeat.
/// </summary>
public class PingSystem : ISystem
{
    private readonly IEventSender eventSender;
    private readonly PingController pingController;
    private readonly ISystemTimer timer;

    /// <summary>
    ///     Whether auto ping is currently enabled.
    /// </summary>
    private bool autoPingEnabled;

    /// <summary>
    ///     Interval in milliseconds between each automatic ping.
    /// </summary>
    private uint autoPingIntervalMs;

    /// <summary>
    ///     System time of last ping.
    /// </summary>
    private ulong lastPingTime;

    public PingSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, ISystemTimer timer,
        PingController pingController, IEventSender eventSender)
    {
        EventCommunicator = eventCommunicator;
        this.timer = timer;
        this.pingController = pingController;
        this.eventSender = eventSender;

        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Core_Ping_Ping,
        EventId.Core_Ping_Pong,
        EventId.Core_Ping_Start,
        EventId.Core_Ping_SetAuto
    };

    public int WorkloadEstimate => 1;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventsProcessed++;
            switch (ev.EventId)
            {
                case EventId.Core_Ping_Start:
                    OnPingStart();
                    break;

                case EventId.Core_Ping_SetAuto:
                    if (ev.EventDetails == null)
                    {
                        Logger.Error("Received SetAuto event without details.");
                        break;
                    }

                    OnSetAuto((AutoPingEventDetails)ev.EventDetails);
                    break;

                case EventId.Core_Ping_Ping:
                    // Ignore local pings as they depart the system.
                    if (!ev.Local)
                        OnPing();
                    break;

                case EventId.Core_Ping_Pong:
                    // Ignore local pongs as they depart the system.
                    if (!ev.Local)
                        OnPong(ev);
                    break;

                default:
                    Logger.ErrorFormat("Received unhandled event with type {0}.", ev.EventId);
                    break;
            }
        }

        return eventsProcessed;
    }

    private void OnPong(Event ev)
    {
        // Compute round trip time.
        var now = timer.GetTime();
        var delta = now - lastPingTime;

        Logger.DebugFormat("Ping roundtrip time for connection {0}: {1} ms",
            ev.FromConnectionId,
            (float)delta / Units.SystemTime.Millisecond);
    }

    private void OnPing()
    {
        // Ping received, respond with a pong.
        pingController.Pong(eventSender);
    }

    private void OnSetAuto(AutoPingEventDetails evEventDetails)
    {
        // Update state.
        var lastState = autoPingEnabled;
        autoPingEnabled = evEventDetails.Enable;
        autoPingIntervalMs = evEventDetails.IntervalMs;

        // If auto ping was just enabled, schedule the first ping.
        if (autoPingEnabled && !lastState)
            pingController.StartPing(eventSender, timer.GetTime() + autoPingIntervalMs * Units.SystemTime.Millisecond);
    }

    private void OnPingStart()
    {
        // Emit a ping.
        pingController.Ping(eventSender);
        lastPingTime = timer.GetTime();

        // If auto ping is enabled, schedule the next ping.
        if (autoPingEnabled)
        {
            var nextTime = lastPingTime + autoPingIntervalMs * Units.SystemTime.Millisecond;
            pingController.StartPing(eventSender, nextTime);
        }
    }
}