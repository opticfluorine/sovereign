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
using Engine8.EngineCore.Events;
using Engine8.EngineUtil.Collections;

namespace Engine8.EngineCore.Systems.EventSystem
{

    /// <summary>
    /// Main event loop provided by the event system.
    /// </summary>
    public class MainEventLoop : IEventLoop
    {

        /// <summary>
        /// Default size of the future event queue.
        /// </summary>
        private const int QUEUE_SIZE = 4096;

        public bool Terminated { get; private set; } = false;

        /// <summary>
        /// The collection of systems known to the event loop.
        /// </summary>
        private readonly ICollection<ISystem> systems;

        /// <summary>
        /// Event communicators listening to each event ID.
        /// </summary>
        private readonly IDictionary<int, IList<EventCommunicator>> communicatorsByEventId;

        /// <summary>
        /// Priority queue of future events ordered by dispatch time.
        /// </summary>
        private readonly IHeap<Event> EventQueue 
            = new BinaryHeap<Event>(QUEUE_SIZE, new EventTimeComparer());

        /// <summary>
        /// The system time of the last update step (microseconds).
        /// </summary>
        private ulong LastUpdateTime;

        /// <summary>
        /// Whether exit is signaled.
        /// </summary>
        private bool exiting = false;

        public MainEventLoop(ICollection<ISystem> systems)
        {
            this.systems = systems;
        }

        public int PumpEventLoop()
        {
            /* Check for events that need to be dispatched. */
            int dispatchCount;
            for (dispatchCount = 0; EventQueue.Count > 0  &&
                EventQueue.Peek().EventTime <= LastUpdateTime; ++dispatchCount)
            {
                
            }
            return dispatchCount;
        }

        /// <summary>
        /// Pumps the event loop and updates the system time.
        /// 
        /// This should only be called from the event system; if the event queue
        /// needs to be pumped without advancing the clock, call PumpEventLoop().
        /// </summary>
        /// <param name="currentTime">Current system time.</param>
        /// <returns>Number of events dispatched.</returns>
        public int PumpEventLoop(ulong currentTime)
        {
            LastUpdateTime = currentTime;
            return PumpEventLoop();
        }

        /// <summary>
        /// Comparer that sorts events by their event time.
        /// </summary>
        private class EventTimeComparer : Comparer<Event>
        {
            public override int Compare(Event x, Event y)
            {
                return x.EventTime.CompareTo(y.EventTime);
            }
        }

    }

}
