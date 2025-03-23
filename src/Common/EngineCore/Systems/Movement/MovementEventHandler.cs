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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Movement;

public class MovementEventHandler
{
    private readonly ILogger<MovementEventHandler> logger;
    private readonly MovementManager manager;
    private readonly CollisionMeshManager meshManager;

    public MovementEventHandler(MovementManager manager, ILogger<MovementEventHandler> logger,
        CollisionMeshManager meshManager)
    {
        this.manager = manager;
        this.logger = logger;
        this.meshManager = meshManager;
    }

    /// <summary>
    ///     Handles a movement event.
    /// </summary>
    /// <param name="ev">Movement event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Core_Movement_Move:
            {
                // Ignore local events that originated here - only process authoritative updates from server.
                if (ev.Local) break;
                if (ev.EventDetails is not MoveEventDetails details)
                {
                    logger.LogError("Received Move event with bad details.");
                    break;
                }

                manager.HandleAuthoritativeMove(details);
            }
                break;

            case EventId.Core_Movement_RequestMove:
            {
                if (ev.EventDetails is not RequestMoveEventDetails details)
                {
                    logger.LogError("Received RequestMove event with bad details.");
                    break;
                }

                manager.HandleRequestMove(details);
            }
                break;

            case EventId.Core_Tick:
                manager.HandleTick();
                meshManager.OnTick();
                break;

            case EventId.Core_Block_GridUpdated:
            {
                if (ev.EventDetails is not BlockPresenceGridUpdatedEventDetails details)
                {
                    logger.LogError("Received GridUpdated event with bad details.");
                    break;
                }

                meshManager.ScheduleUpdate(details.WorldSegmentIndex, details.Z);
                break;
            }
        }
    }
}