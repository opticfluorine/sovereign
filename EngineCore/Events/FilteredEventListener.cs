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
    /// Listens for events and applies a user-specified filter to dynamically
    /// remove unwanted events.
    /// </summary>
    public class FilteredEventListener : IEventListener
    {

        /// <summary>
        /// Event IDs of interest.
        /// </summary>
        private readonly ISet<int> listenIds;

        /// <summary>
        /// Callback delegate invoked when an event is accepted by the filter.
        /// </summary>
        private readonly Action<Event, IEventDispatcher> callbackDelegate;

        /// <summary>
        /// Delegate invoked to filter a collection of dispatched events.
        /// </summary>
        private readonly Func<Event, bool> filterDelegate;

        /// <summary>
        /// Creates a new filtered event listener.
        /// </summary>
        /// <param name="ListenIds">Event IDs of interest.</param>
        /// <param name="CallbackDelegate">Delegate invoked when an event is accepted by the filter.</param>
        /// <param name="FilterDelegate">Delegate invoked to filter a collection of dispatched events.</param>
        public FilteredEventListener(ISet<int> ListenIds, Action<Event, IEventDispatcher> CallbackDelegate,
            Func<Event, bool> FilterDelegate)
        {
            // make a copy in case the original list is changed later
            listenIds = new HashSet<int>(ListenIds);

            callbackDelegate = CallbackDelegate;
            filterDelegate = FilterDelegate;
        }

        public ISet<int> EventIdsListening { get => listenIds; }

        /// <summary>
        /// Filters a collection of events, then invokes the callback for each accepted event.
        /// </summary>
        /// <param name="ev">Dispatched event.</param>
        /// <param name="dispatcher">Dispatcher.</param>
        public void Notify(Event ev, IEventDispatcher dispatcher)
        {
            // Check if the event passes the filter
            if (filterDelegate.Invoke(ev))
            {
                callbackDelegate.Invoke(ev, dispatcher);
            }
        }

    }

}
