/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Movement.Events;

namespace Sovereign.EngineCore.Systems.Movement
{

    /// <summary>
    /// System responsible for coordinating the movement of entities.
    /// </summary>
    public class MovementSystem : ISystem, IDisposable
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public EventCommunicator EventCommunicator { get; private set; }

        public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>()
        {
            EventId.Core_Move_Once,
            EventId.Core_Set_Velocity,
            EventId.Core_End_Movement,
        };

        public int WorkloadEstimate { get; } = 80;

        private readonly VelocityManager velocityManager;
        private readonly IEventLoop eventLoop;

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

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

        public void Cleanup()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void ExecuteOnce()
        {
            /* Poll for movement-related events. */
            while (EventCommunicator.GetIncomingEvent(out Event ev))
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
            }
        }

        /// <summary>
        /// Handler for Core_Set_Velocity events.
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
        /// Handler for Core_End_Movement events.
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
        /// Handler for Core_Move_Once events.
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

}
