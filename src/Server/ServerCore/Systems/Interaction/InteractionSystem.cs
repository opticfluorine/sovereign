// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

namespace Sovereign.ServerCore.Systems.Interaction;

internal sealed class InteractionSystem : ISystem
{
    private readonly InteractionHandler handler;
    private readonly ILogger<InteractionSystem> logger;

    public InteractionSystem(EventCommunicator eventCommunicator, ILogger<InteractionSystem> logger,
        InteractionHandler handler, IEventLoop eventLoop)
    {
        this.logger = logger;
        this.handler = handler;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Interaction_Interact
    };

    public int WorkloadEstimate { get; } = 40;

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
            count++;
            switch (ev.EventId)
            {
                case EventId.Core_Interaction_Interact:
                {
                    if (ev.EventDetails is not InteractEventDetails details)
                    {
                        logger.LogError("Received Interact without details.");
                        break;
                    }

                    handler.HandleInteraction(details.SourceEntityId, details.TargetEntityId, details.ToolEntityId);
                    break;
                }
            }
        }

        return count;
    }
}