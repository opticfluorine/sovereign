// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.Perspective;

internal class PerspectiveSystem : ISystem
{
    private readonly PerspectiveLineManager lineManager;
    private readonly ILogger<PerspectiveSystem> logger;
    private readonly OverheadBlockGraphManager overheadBlockGraphManager;

    public PerspectiveSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        PerspectiveLineManager lineManager, ILogger<PerspectiveSystem> logger,
        OverheadBlockGraphManager overheadBlockGraphManager)
    {
        this.lineManager = lineManager;
        this.logger = logger;
        this.overheadBlockGraphManager = overheadBlockGraphManager;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Core_WorldManagement_Unsubscribe
    };

    public int WorkloadEstimate => 120;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var count = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            switch (ev.EventId)
            {
                case EventId.Core_Tick:
                    overheadBlockGraphManager.OnTick();
                    break;

                case EventId.Core_WorldManagement_Subscribe:
                {
                    if (ev.EventDetails is not WorldSegmentSubscriptionEventDetails subDetails)
                    {
                        logger.LogError("Bad details for Subscribe event.");
                        break;
                    }

                    lineManager.OnWorldSegmentSubscribe(subDetails.SegmentIndex);
                }
                    break;

                case EventId.Core_WorldManagement_Unsubscribe:
                {
                    if (ev.EventDetails is not WorldSegmentSubscriptionEventDetails subDetails)
                    {
                        logger.LogError("Bad details for Unsubscribe event.");
                        break;
                    }

                    lineManager.OnWorldSegmentUnsubscribe(subDetails.SegmentIndex);
                }
                    break;
            }

            count++;
        }

        return count;
    }
}