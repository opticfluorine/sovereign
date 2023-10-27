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
using Sovereign.EngineCore.Systems.Movement.Events;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     System responsible for coordinating the movement of entities.
/// </summary>
public class MovementSystem : ISystem, IDisposable
{
    private readonly IEventLoop eventLoop;

    private readonly VelocityManager velocityManager;

    public MovementSystem(VelocityManager velocityManager, IEventLoop eventLoop,
        EventCommunicator eventCommunicator, EventDescriptions eventDescriptions)
    {
        /* Dependency injection. */
        this.velocityManager = velocityManager;
        this.eventLoop = eventLoop;
        EventCommunicator = eventCommunicator;

        /* Register events. */
        eventDescriptions.RegisterEvent<MoveOnceEventDetails>(EventId.Core_Move_Once);
        eventDescriptions.RegisterEvent<SetVelocityEventDetails>(EventId.Core_Set_Velocity);
        eventDescriptions.RegisterEvent<EntityEventDetails>(EventId.Core_End_Movement);

        /* Register system. */
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Dispose()
    {
        eventLoop.UnregisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Move_Once,
        EventId.Core_Set_Velocity,
        EventId.Core_End_Movement
    };

    public int WorkloadEstimate { get; } = 80;

    public void Cleanup()
    {
    }

    public void Initialize()
    {
    }

    public int ExecuteOnce()
    {
        /* Poll for movement-related events. */
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            switch (ev.EventId)
            {
                /* Handle direct movements. */
                case EventId.Core_Move_Once:
                    /* TODO: Implement */
                    break;

                /* Handle velocity changes. */
                case EventId.Core_Set_Velocity:
                    OnSetVelocity(ev);
                    break;

                /* Stop movement. */
                case EventId.Core_End_Movement:
                    /* TODO: Implement */
                    break;

                /* Ignore unhandled events. */
                default:
                    Logger.WarnFormat("Unhandled event with ID = {0}.", ev.EventId);
                    break;
            }

            eventsProcessed++;
        }

        return eventsProcessed;
    }

    /// <summary>
    ///     Handler for Core_Set_Velocity events.
    /// </summary>
    /// <param name="ev">Core_Set_Velocity event.</param>
    private void OnSetVelocity(Event ev)
    {
        /* Get details. */
        SetVelocityEventDetails details;
        try
        {
            details = (SetVelocityEventDetails)ev.EventDetails;
        }
        catch (InvalidCastException e)
        {
            /* Log error and discard event. */
            Logger.Warn("Bad Core_Set_Velocity event.", e);
            return;
        }

        /* Handle event. */
        velocityManager.SetVelocity(details.EntityId, details.RateX, details.RateY,
            ev.EventTime);
    }

    /// <summary>
    ///     Handler for Core_End_Movement events.
    /// </summary>
    /// <param name="ev">Core_End_Movement event.</param>
    private void OnEndMovement(Event ev)
    {
        /* Extract details. */
        EntityEventDetails details;
        try
        {
            details = (EntityEventDetails)ev.EventDetails;
        }
        catch (InvalidCastException e)
        {
            /* Log error and discard event. */
            Logger.Warn("Bad Core_End_Movement event.", e);
            return;
        }

        /* Handle event. */
        velocityManager.StopMovement(details.EntityId);
    }

    /// <summary>
    ///     Handler for Core_Move_Once events.
    /// </summary>
    /// <param name="ev">Core_Move_Once event.</param>
    private void OnMoveOnce(Event ev)
    {
        /* Extract details. */
        MoveOnceEventDetails details;
        try
        {
            details = (MoveOnceEventDetails)ev.EventDetails;
        }
        catch (InvalidCastException e)
        {
            /* Log error and discard event. */
            Logger.Warn("Bad Core_Move_Once event.", e);
            return;
        }

        /* Handle event. */
        velocityManager.MoveOnce(details.EntityId, details.MovementPhase,
            ev.EventTime);
    }
}