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
        private const int QUEUE_SIZE = 512;

        public bool Terminated { get; private set; } = false;

        /// <summary>
        /// Registered event listeners.
        /// </summary>
        private readonly ISet<IEventListener> RegisteredListeners 
            = new HashSet<IEventListener>();

        /// <summary>
        /// Map from event IDs to interested listeners.
        /// </summary>
        private readonly IDictionary<int, ISet<IEventListener>> TypeListenerMap
            = new Dictionary<int, ISet<IEventListener>>();

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

        public void DispatchEvent(Event ev)
        {
            /* Sanity check. */
            if (ev == null)
            {
                throw new ArgumentNullException(nameof(ev));
            }

            /*
             * Even if the event is immediate, enqueue it to be sent
             * the next time the event loop is pumped to avoid stack issues.
             */
            EnqueueFutureEvent(ev);
        }

        public void DispatchEvents(IEnumerable<Event> evs)
        {
            /* Sanity check. */
            if (evs == null)
                throw new ArgumentNullException(nameof(evs));

            /* Dispatch all events. */
            foreach (Event ev in evs)
            {
                DispatchEvent(ev);
            }
        }

        public int PumpEventLoop()
        {
            /* Check for events that need to be dispatched. */
            int dispatchCount;
            for (dispatchCount = 0; EventQueue.Count > 0  &&
                EventQueue.Peek().EventTime <= LastUpdateTime; ++dispatchCount)
            {
                ImmediatelyDispatchEvent(EventQueue.Pop());
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

        public void RegisterListener(IEventListener listener)
        {
            /* Sanity check. */
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }
            if (RegisteredListeners.Contains(listener)) return;

            /* Register the listener and add it to the type maps. */
            RegisteredListeners.Add(listener);
            foreach (int id in listener.EventIdsListening)
            {
                AddListenerToTypeSet(listener, id);
            }
        }

        public void DeregisterListener(IEventListener listener)
        {
            /* Sanity check. */
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            if (!RegisteredListeners.Contains(listener)) return;

            /* Deregister the listener. */
            foreach (int id in listener.EventIdsListening)
            {
                TypeListenerMap[id].Remove(listener);
            }
            RegisteredListeners.Remove(listener);
        }

        /// <summary>
        /// Enqueues an event to be dispatched in the future.
        /// </summary>
        /// <param name="ev">Event to be dispatched.</param>
        private void EnqueueFutureEvent(Event ev)
        {
            /* Push the event onto the heap. */
            EventQueue.Push(ev);
        }

        /// <summary>
        /// Immediately dispatches the given event.
        /// </summary>
        /// <param name="ev">Event to be dispatched.</param>
        private void ImmediatelyDispatchEvent(Event ev)
        {
            /* Check if there are any interested listeners. */
            if (!TypeListenerMap.ContainsKey(ev.EventId)) return;

            /* Dispatch the event to all interested listeners. */
            foreach (IEventListener listener in TypeListenerMap[ev.EventId])
            {
                listener.Notify(ev, this);
            }
        }

        /// <summary>
        /// Adds a listener to the type set for the given event ID.
        /// </summary>
        /// <param name="listener">Listener to be added.</param>
        /// <param name="id">Event ID.</param>
        private void AddListenerToTypeSet(IEventListener listener, int id)
        {
            /* Create the type set if this is the first listener. */
            if (!TypeListenerMap.ContainsKey(id))
            {
                TypeListenerMap[id] = new HashSet<IEventListener>();
            }

            /* Register the listener. */
            TypeListenerMap[id].Add(listener);
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
