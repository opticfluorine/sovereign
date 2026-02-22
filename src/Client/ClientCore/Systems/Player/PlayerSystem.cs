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
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.Player;

/// <summary>
///     System that handles player-centered client side interactions.
/// </summary>
public class PlayerSystem : ISystem
{
    private readonly PlayerInteractionHandler interactionHandler;
    private readonly PlayerInventoryActions inventoryActions;
    private readonly ILogger<PlayerSystem> logger;

    public PlayerSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        ILogger<PlayerSystem> logger, PlayerInventoryActions inventoryActions,
        PlayerInteractionHandler interactionHandler)
    {
        this.logger = logger;
        this.inventoryActions = inventoryActions;
        this.interactionHandler = interactionHandler;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Client_Player_PickUpItemUnder,
        EventId.Client_Player_Interact
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
        var count = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            switch (ev.EventId)
            {
                case EventId.Client_Player_PickUpItemUnder:
                    inventoryActions.PickUpItemUnder();
                    break;

                case EventId.Client_Player_Interact:
                    interactionHandler.Interact();
                    break;

                default:
                    logger.LogError("Unrecognized event {EventId} in PlayerSystem.", ev.EventId);
                    break;
            }

            count++;
        }

        return count;
    }
}