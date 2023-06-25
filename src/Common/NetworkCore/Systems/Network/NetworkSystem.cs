/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Network.Service;

namespace Sovereign.NetworkCore.Systems.Network;

/// <summary>
///     System that connects the event loop to the networking service.
/// </summary>
public sealed class NetworkSystem : ISystem
{
    private readonly NetworkingService networkingService;
    private readonly OutboundNetworkPipeline outboundPipeline;

    public NetworkSystem(NetworkingService networkingService,
        EventCommunicator eventCommunicator,
        IEventLoop eventLoop,
        NetworkEventAdapter eventAdapter,
        OutboundNetworkPipeline outboundPipeline)
    {
        this.networkingService = networkingService;
        EventCommunicator = eventCommunicator;
        this.outboundPipeline = outboundPipeline;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    /// <summary>
    ///     Event IDs that are candidates to be replicated onto the network.
    /// </summary>
    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Set_Velocity,
        EventId.Core_Move_Once,
        EventId.Core_End_Movement
    };

    public int WorkloadEstimate => 50;

    public void Cleanup()
    {
        networkingService.Stop();
    }

    public int ExecuteOnce()
    {
        /* Process outgoing local events. */
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            /* Discard nonlocal events. */
            if (!ev.Local) continue;

            /* Pass what's left to the outbound pipeline. */
            outboundPipeline.ProcessEvent(ev);

            eventsProcessed++;
        }

        return eventsProcessed;
    }

    public void Initialize()
    {
        networkingService.Start();
    }
}