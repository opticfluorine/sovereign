/*
 * Engine8 Dynamic World MMORPG Engine
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
using Engine8.EngineCore.Events;

namespace Engine8.EngineCore.Systems.Movement
{

    /// <summary>
    /// System responsible for coordinating the movement of entities.
    /// </summary>
    public class MovementSystem : ISystem
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public EventCommunicator EventCommunicator { get; set; }

        public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>()
        {
            EventId.Core_Move_Once,
            EventId.Core_Set_Velocity,
            EventId.Core_End_Movement,
        };

        public int WorkloadEstimate { get; } = 80;

        public void Cleanup()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void ExecuteOnce()
        {
            /* Poll for movement-related events. */
            Event ev;
            while (EventCommunicator.GetIncomingEvent(out ev))
            {
                switch (ev.EventId)
                {
                    /* Handle direct movements. */
                    case EventId.Core_Move_Once:
                        /* TODO: Implement */
                        break;

                    /* Handle velocity changes. */
                    case EventId.Core_Set_Velocity:
                        /* TODO: Implement */
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

    }

}
