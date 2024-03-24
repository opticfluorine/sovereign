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
using Castle.Core.Logging;
using Sovereign.ClientCore.Events;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     System responsible for managing the top-level client state machine.
/// </summary>
public class ClientStateSystem : ISystem
{
    private readonly IEventLoop eventLoop;
    private readonly ClientStateFlagManager flagManager;
    private readonly MainMenuStateMachine mainMenuStateMachine;
    private readonly PlayerStateManager playerStateManager;
    private readonly ClientStateMachine stateMachine;
    private readonly WorldEntryDetector worldEntryDetector;

    public ClientStateSystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        WorldEntryDetector worldEntryDetector, ClientStateFlagManager flagManager,
        PlayerStateManager playerStateManager, ClientStateMachine stateMachine,
        MainMenuStateMachine mainMenuStateMachine)
    {
        this.eventLoop = eventLoop;
        this.worldEntryDetector = worldEntryDetector;
        this.flagManager = flagManager;
        this.playerStateManager = playerStateManager;
        this.stateMachine = stateMachine;
        this.mainMenuStateMachine = mainMenuStateMachine;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>
    {
        EventId.Client_Network_BeginConnection,
        EventId.Client_Network_PlayerEntitySelected,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Client_State_WorldSegmentLoaded,
        EventId.Client_State_SetFlag,
        EventId.Core_Network_Logout,
        EventId.Client_State_SetMainMenuState,
        EventId.Client_Network_ConnectionLost
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
                {
                    if (ev.EventDetails is not EntityEventDetails details)
                    {
                        Logger.Error("Received OnPlayerSelected without details.");
                        break;
                    }

                    playerStateManager.PlayerSelected(details.EntityId);
                }
                    worldEntryDetector.OnPlayerSelected();
                    break;

                case EventId.Core_Network_Logout:
                    OnLogout();
                    break;

                case EventId.Client_Network_ConnectionLost:
                    OnConnectionLost();
                    break;

                case EventId.Core_WorldManagement_Subscribe:
                    worldEntryDetector.OnSegmentSubscribe();
                    break;

                case EventId.Client_State_WorldSegmentLoaded:
                    worldEntryDetector.OnSegmentLoaded();
                    break;

                case EventId.Client_State_SetFlag:
                {
                    if (ev.EventDetails is not ClientStateFlagEventDetails details)
                    {
                        Logger.Error("Received SetFlag event without details.");
                        break;
                    }

                    flagManager.SetStateFlagValue(details.Flag, details.NewValue);
                }
                    break;

                case EventId.Client_State_SetMainMenuState:
                {
                    if (ev.EventDetails is not MainMenuEventDetails details)
                    {
                        Logger.Error("Received SetMainMenuState event without details.");
                        break;
                    }

                    mainMenuStateMachine.SetState(details.MainMenuState);
                }
                    break;
            }
        }

        return processed;
    }

    /// <summary>
    ///     Called when the player logs out to player selection.
    /// </summary>
    private void OnLogout()
    {
        mainMenuStateMachine.SetState(MainMenuState.PlayerSelection);
        ClearStateOnLogout();
    }

    /// <summary>
    ///     Called when the connection to the server is lost.
    /// </summary>
    private void OnConnectionLost()
    {
        mainMenuStateMachine.SetState(MainMenuState.ConnectionLost);
        ClearStateOnLogout();
    }

    private void ClearStateOnLogout()
    {
        playerStateManager.PlayerLogout();
        stateMachine.TryTransition(MainClientState.MainMenu);
        flagManager.ResetFlags();
    }
}