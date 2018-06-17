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

using System.Collections.Generic;
using System.Linq;
using Engine8.EngineCore.Components;
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
        /// The collection of event senders to listen on.
        /// </summary>
        private readonly ICollection<EventSender> eventSenders;

        /// <summary>
        /// The collection of event adapters.
        /// </summary>
        private readonly ICollection<IEventAdapter> eventAdapters;

        /// <summary>
        /// Component manager.
        /// </summary>
        private readonly ComponentManager componentManager;

        /// <summary>
        /// Event communicators listening to each event ID.
        /// </summary>
        private readonly IDictionary<EventId, List<EventCommunicator>> communicatorsByEventId
            = new Dictionary<EventId, List<EventCommunicator>>();

        /// <summary>
        /// Priority queue of future events ordered by dispatch time.
        /// </summary>
        private readonly IHeap<Event> FutureEventQueue 
            = new BinaryHeap<Event>(QUEUE_SIZE, new EventTimeComparer());

        /// <summary>
        /// The system time of the last update step (microseconds).
        /// </summary>
        private ulong LastUpdateTime;

        public MainEventLoop(ICollection<ISystem> systems, 
            ICollection<EventSender> eventSenders,
            ICollection<IEventAdapter> eventAdapters,
            ComponentManager componentManager)
        {
            /* Set dependencies. */
            this.systems = systems;
            this.eventSenders = eventSenders;
            this.eventAdapters = eventAdapters;
            this.componentManager = componentManager;

            /* Build data structures. */
            BuildCommunicatorTables();
        }

        public void PumpEventLoop()
        {
            /* Retrieve pending events from the communicators. */
            RetrievePendingEvents();

            /* Retrieve events from the event adapters. */
            RetrieveAdaptedEvents();
        }

        /// <summary>
        /// Updates the system time to the next tick if needed.
        /// </summary>
        public void UpdateSystemTime(ulong systemTime)
        {
            /* Merge the component updates. */
            componentManager.UpdateAllComponents();

            /* Advance the system time. */
            LastUpdateTime = systemTime;

            /* Dispatch events enqueued for dispatch by this time. */
            DispatchEnqueuedEvents();
        }

        /// <summary>
        /// Builds the communicator tables.
        /// </summary>
        private void BuildCommunicatorTables()
        {
            /* Enumerate all event IDs that will be listened for. */
            var eventIds = from id in systems
                           .SelectMany(system => system.EventIdsOfInterest)
                           .Distinct()
                           select id;

            /* Find the communicators associated with each event ID. */
            foreach (var eventId in eventIds)
            {
                var comms = from system in systems
                            .Where(s => s.EventIdsOfInterest.Contains(eventId))
                            select system.EventCommunicator;
                var list = new List<EventCommunicator>();
                list.AddRange(comms);
                communicatorsByEventId.Add(eventId, list);
            }
        }

        /// <summary>
        /// Retrieves pending events from the communicators.
        /// </summary>
        private void RetrievePendingEvents()
        {
            foreach (var comm in eventSenders)
            {
                Event nextEvent;
                do
                {
                    nextEvent = comm.GetOutgoingEvent();
                    if (nextEvent != null)
                    {
                        EnqueueEvent(nextEvent);
                    }
                }
                while (nextEvent != null);
            }
        }
        
        /// <summary>
        /// Retrieves and enqueues all available events from the IEventAdapters.
        /// </summary>
        private void RetrieveAdaptedEvents()
        {
            foreach (IEventAdapter eventAdapter in eventAdapters)
            {
                eventAdapter.PrepareEvents();

                Event ev;
                do
                {
                    ev = eventAdapter.PollEvent();
                    if (ev != null)
                    {
                        EnqueueEvent(ev);
                    }
                }
                while (ev != null);
            }
        }

        /// <summary>
        /// Enqueues an event.
        /// </summary>
        /// <param name="ev">Event to enqueue.</param>
        private void EnqueueEvent(Event ev)
        {
            /* Is the event to be dispatched immediately? */
            if (ev.EventTime <= LastUpdateTime)
            {
                /* Dispatch immediately. */
                DispatchImmediateEvent(ev);
            }
            else
            {
                /* Enqueue for later dispatch. */
                FutureEventQueue.Add(ev);
            }
        }

        /// <summary>
        /// Immediately dispatches the given event.
        /// </summary>
        /// <param name="ev">Event to be immediately dispatched.</param>
        private void DispatchImmediateEvent(Event ev)
        {
            var eventId = ev.EventId;

            /* Handle Core_Quit events specially. */
            if (eventId == EventId.Core_Quit)
            {
                Terminated = true;
            }

            /* Set the event time to the current tick time. */
            ev.EventTime = LastUpdateTime;

            /* Dispatch to all interested communicators, if any.. */
            if (communicatorsByEventId.ContainsKey(eventId))
            {
                foreach (var comm in communicatorsByEventId[eventId])
                {
                    comm.SendEventToSystem(ev);
                }
            }   
        }

        /// <summary>
        /// Dispatches all enqueued events that are scheduled for delivery
        /// no later than the present system time.
        /// </summary>
        /// <returns>Number of dispatched events.</returns>
        private void DispatchEnqueuedEvents()
        {
            while (FutureEventQueue.Count > 0 &&
                FutureEventQueue.Peek().EventTime <= LastUpdateTime)
            {
                var nextEvent = FutureEventQueue.Pop();
                DispatchImmediateEvent(nextEvent);
            }
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
