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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Pathfinding;

/// <summary>
///     System responsible for performing pathfinding between world points.
/// </summary>
internal class PathfindingSystem : ISystem
{
    public PathfindingSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop)
    {
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }
    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>();
    public int WorkloadEstimate => 30;

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
        }

        return count;
    }
}