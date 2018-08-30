/*
 * Sovereign Dynamic World MMORPG Engine
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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.Input
{

    /// <summary>
    /// System responsible for handling user input.
    /// </summary>
    public class InputSystem : ISystem
    {

        public EventCommunicator EventCommunicator { get; set; }

        public ISet<EventId> EventIdsOfInterest { get; }
           = new HashSet<EventId>() {
               EventId.Client_Input_KeyUp,
               EventId.Client_Input_KeyDown,
           };

        public int WorkloadEstimate { get; } = 50;

        /// <summary>
        /// Keyboard event handler.
        /// </summary>
        private readonly KeyboardEventHandler keyboardEventHandler;

        public InputSystem(KeyboardEventHandler keyboardEventHandler)
        {
            this.keyboardEventHandler = keyboardEventHandler;
        }

        public void Initialize()
        {
            
        }

        public void Cleanup()
        {
            
        }

        public void ExecuteOnce()
        {
            /* Poll for events. */
            Event ev;
            while (EventCommunicator.GetIncomingEvent(out ev))
            {
                switch (ev.EventId)
                {
                    /* Route keyboard events appropriately. */
                    case EventId.Client_Input_KeyUp:
                    case EventId.Client_Input_KeyDown:
                        keyboardEventHandler.HandleEvent(ev);
                        break;

                    /* Ignore other events. */
                    default:
                        break;
                }
            }
        }

    }

}
