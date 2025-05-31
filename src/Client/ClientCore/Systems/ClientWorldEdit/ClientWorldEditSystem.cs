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
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     System that manages the client-side world editor function.
/// </summary>
public class ClientWorldEditSystem : ISystem
{
    private readonly ClientStateServices clientStateServices;
    private readonly ClientWorldEditInputHandler inputHandler;
    private readonly ILogger<ClientWorldEditSystem> logger;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ClientWorldEditState state;

    public ClientWorldEditSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, PlayerRoleCheck roleCheck,
        ClientStateServices clientStateServices, ClientWorldEditState state, ClientWorldEditInputHandler inputHandler,
        ILogger<ClientWorldEditSystem> logger)
    {
        this.roleCheck = roleCheck;
        this.clientStateServices = clientStateServices;
        this.state = state;
        this.inputHandler = inputHandler;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Client_Input_MouseWheelTick,
        EventId.Core_Tick,
        EventId.Client_WorldEdit_SetZOffset,
        EventId.Client_WorldEdit_SetPenWidth
    };

    public int WorkloadEstimate { get; } = 5;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var count = 0;

        // Only process events while the world edit mode is active and the player has the Admin role.
        // Otherwise still consume the events, but just discard them.
        var shouldProcess = clientStateServices.TryGetSelectedPlayer(out var playerEntityId)
                            && clientStateServices.GetStateFlagValue(ClientStateFlag.WorldEditMode)
                            && roleCheck.IsPlayerAdmin(playerEntityId);

        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            count++;
            if (!shouldProcess) continue;

            switch (ev.EventId)
            {
                case EventId.Client_Input_MouseWheelTick:
                {
                    if (ev.EventDetails is not BooleanEventDetails details)
                    {
                        logger.LogWarning("Received MouseWheelTick without details.");
                        break;
                    }

                    state.OnScrollTick(details.Value);
                    break;
                }

                case EventId.Client_WorldEdit_SetZOffset:
                {
                    if (ev.EventDetails is not GenericEventDetails<int> details)
                    {
                        logger.LogWarning("Received SetZOffset without details.");
                        break;
                    }

                    state.SetZOffset(details.Value);
                    break;
                }
                
                case EventId.Client_WorldEdit_SetPenWidth:
                {
                    if (ev.EventDetails is not GenericEventDetails<int> details)
                    {
                        logger.LogWarning("Received SetPenWidth without details.");
                        break;
                    }

                    state.SetPenWidth(details.Value);
                    break;
                }

                case EventId.Core_Tick:
                    inputHandler.OnTick();
                    break;
            }
        }

        return count;
    }
}