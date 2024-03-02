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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     System responsible for managing the top-level client state machine.
/// </summary>
public class ClientStateSystem : ISystem
{
    private readonly IEventLoop eventLoop;
    private readonly WorldEntryDetector worldEntryDetector;

    public ClientStateSystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        WorldEntryDetector worldEntryDetector)
    {
        this.eventLoop = eventLoop;
        this.worldEntryDetector = worldEntryDetector;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Client_Network_BeginConnection,
        EventId.Client_Network_PlayerEntitySelected,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Client_State_WorldSegmentLoaded
    };

    public int WorkloadEstimate => 10;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var processed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            processed++;
            switch (ev.EventId)
            {
                case EventId.Client_Network_BeginConnection:
                    worldEntryDetector.OnLogin();
                    break;

                case EventId.Client_Network_PlayerEntitySelected:
                    worldEntryDetector.OnPlayerSelected();
                    break;

                case EventId.Core_WorldManagement_Subscribe:
                    worldEntryDetector.OnSegmentSubscribe();
                    break;

                case EventId.Client_State_WorldSegmentLoaded:
                    worldEntryDetector.OnSegmentLoaded();
                    break;
            }
        }

        return processed;
    }
}