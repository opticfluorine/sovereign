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

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// Simple event listener that does not filter on events.
    /// </summary>
    public class SimpleEventListener : IEventListener
    {
        /// <summary>
        /// Event IDs to listen for.
        /// </summary>
        private readonly ISet<int> listenIds;

        private readonly Action<Event, IEventDispatcher> callbackAction;

        /// <summary>
        /// Creates a new SimpleEventListener object.
        /// </summary>
        /// <param name="ListenIds">Set of event IDs of interest.</param>
        /// <param name="CallbackAction">Delegate called when an event is received.</param>
        public SimpleEventListener(ISet<int> ListenIds, Action<Event, IEventDispatcher> CallbackAction)
        {
            // make a copy in case the original is modified later
            listenIds = new HashSet<int>(ListenIds);

            callbackAction = CallbackAction;
        }

        /// <summary>
        /// Gets the event IDs of interest.
        /// </summary>
        /// <returns>Set of event IDs of interest.</returns>
        public ISet<int> EventIdsListening { get => listenIds; }

        /// <summary>
        /// Called when events are delivered to the listener.
        /// </summary>
        /// <param name="ev">Event being dispatched.</param>
        /// <param name="dispatcher">Associated event dispatcher.</param>
        public void Notify(Event ev, IEventDispatcher dispatcher)
        {
            callbackAction.Invoke(ev, dispatcher);
        }

    }

}
