/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     System responsible for handling user input.
/// </summary>
public class InputSystem : ISystem, IDisposable
{
    private readonly IEventLoop eventLoop;

    private readonly KeyboardEventHandler keyboardEventHandler;
    private readonly PlayerInputMovementMapper movementMapper;

    public InputSystem(KeyboardEventHandler keyboardEventHandler,
        IEventLoop eventLoop, EventCommunicator eventCommunicator, PlayerInputMovementMapper movementMapper)
    {
        /* Dependency injection. */
        this.keyboardEventHandler = keyboardEventHandler;
        this.eventLoop = eventLoop;
        EventCommunicator = eventCommunicator;
        this.movementMapper = movementMapper;

        /* Register system. */
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; }
        = new HashSet<EventId>
        {
            EventId.Client_Input_KeyUp,
            EventId.Client_Input_KeyDown,
            EventId.Client_Network_PlayerEntitySelected,
            EventId.Core_Tick
        };

    public int WorkloadEstimate { get; } = 50;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        /* Poll for events. */
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            switch (ev.EventId)
            {
                /* Route keyboard events appropriately. */
                case EventId.Client_Input_KeyUp:
                case EventId.Client_Input_KeyDown:
                    keyboardEventHandler.HandleEvent(ev);
                    break;

                case EventId.Core_Tick:
                    movementMapper.OnTick();
                    break;

                case EventId.Client_Network_PlayerEntitySelected:
                    if (ev.EventDetails is not EntityEventDetails)
                    {
                        Logger.Error("Received PlayerEntitySelected with bad details.");
                        break;
                    }

                    movementMapper.SelectPlayer((EntityEventDetails)ev.EventDetails);
                    break;

                /* Ignore other events. */
            }

            eventsProcessed++;
        }

        return eventsProcessed;
    }
}