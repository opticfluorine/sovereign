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
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     System responsible for inventory management across all entities.
/// </summary>
internal sealed class InventorySystem : ISystem
{
    private readonly ILogger<InventorySystem> logger;
    private readonly InventoryManager manager;

    public InventorySystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        ILogger<InventorySystem> logger, InventoryManager manager)
    {
        this.logger = logger;
        this.manager = manager;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Inventory_PickUp,
        EventId.Core_Inventory_Drop,
        EventId.Core_Inventory_DropAtPosition,
        EventId.Core_Inventory_Swap
    };

    public int WorkloadEstimate => 50;

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

            // All non-local inventory events require associated player.
            if (ev is { Local: false, FromPlayerId: 0 })
            {
                logger.LogWarning("Received inventory event {EvId} from connection {CnId} with no player.",
                    ev.EventId, ev.FromConnectionId);
                continue;
            }

            switch (ev.EventId)
            {
                case EventId.Core_Inventory_PickUp:
                {
                    if (ev.EventDetails is not EntityEventDetails details)
                    {
                        logger.LogError("Received PickUp without details.");
                        break;
                    }

                    manager.PickUpItem(ev.FromPlayerId, details.EntityId);
                    break;
                }

                case EventId.Core_Inventory_Drop:
                {
                    if (ev.EventDetails is not IntEventDetails details)
                    {
                        logger.LogError("Received Drop without details.");
                        break;
                    }

                    manager.DropItem(ev.FromPlayerId, (int)details.Value);
                    break;
                }

                case EventId.Core_Inventory_DropAtPosition:
                {
                    if (ev.EventDetails is not IntVectorEventDetails details)
                    {
                        logger.LogError("Received DropAtPosition without details.");
                        break;
                    }

                    manager.DropItemAtPosition(ev.FromPlayerId, details.IntValue, details.VectorValue);
                    break;
                }

                case EventId.Core_Inventory_Swap:
                {
                    if (ev.EventDetails is not IntPairEventDetails details)
                    {
                        logger.LogError("Received Swap without details.");
                        break;
                    }

                    manager.SwapItems(ev.FromPlayerId, details.First, details.Second);
                    break;
                }
            }
        }

        return count;
    }
}