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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.EngineCore.Systems.EventSystem
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
        private readonly ISet<ISystem> systems = new HashSet<ISystem>();

        /// <summary>
        /// The collection of event senders to listen on.
        /// </summary>
        private readonly ISet<IEventSender> eventSenders = new HashSet<IEventSender>();

        /// <summary>
        /// Event adapter manager.
        /// </summary>
        private readonly EventAdapterManager eventAdapterManager;

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
        private readonly IHeap<Event> futureEventQueue
            = new BinaryHeap<Event>(QUEUE_SIZE, new EventTimeComparer());

        /// <summary>
        /// The system time of the last update step (microseconds).
        /// </summary>
        private ulong lastUpdateTime;

        public MainEventLoop(ComponentManager componentManager,
            EventAdapterManager eventAdapterManager)
        {
            this.componentManager = componentManager;
            this.eventAdapterManager = eventAdapterManager;
        }

        public int PumpEventLoop()
        {
            /* Retrieve pending events from the communicators. */
            var eventsProcessed = RetrievePendingEvents();

            /* Retrieve events from the event adapters. */
            eventsProcessed += RetrieveAdaptedEvents();

            return eventsProcessed;
        }

        /// <summary>
        /// Updates the system time to the next tick if needed.
        /// </summary>
        public void UpdateSystemTime(ulong systemTime)
        {
            /* Merge the component updates. */
            componentManager.UpdateAllComponents();

            /* Advance the system time. */
            lastUpdateTime = systemTime;

            /* Emit the Core_Tick event. */
            EmitTickEvent();

            /* Dispatch events enqueued for dispatch by this time. */
            DispatchEnqueuedEvents();
        }

        public void RegisterEventSender(IEventSender eventSender)
        {
            lock (eventSenders)
            {
                eventSenders.Add(eventSender);
            }
        }

        public void UnregisterEventSender(IEventSender eventSender)
        {
            lock (eventSenders)
            {
                eventSenders.Remove(eventSender);
            }
        }

        public void RegisterSystem(ISystem system)
        {
            lock (systems)
            {
                systems.Add(system);
                UpdateCommunicatorTables(system);
            }
        }

        public void UnregisterSystem(ISystem system)
        {
            lock (systems)
            {
                systems.Remove(system);
            }
        }

        /// <summary>
        /// Emits the tick event at the beginning of a tick.
        /// </summary>
        private void EmitTickEvent()
        {
            // Emit tick.
            var ev = new Event(EventId.Core_Tick);
            EnqueueEvent(ev);

            // Emit per-tick performance events.
            var latencyEv = new Event(EventId.Core_Performance_EventLatencyTest,
                new TimeEventDetails() { SystemTime = lastUpdateTime });
            EnqueueEvent(latencyEv);
        }

        /// <summary>
        /// Builds the communicator tables.
        /// </summary>
        /// <param name="system">System.</param>
        private void UpdateCommunicatorTables(ISystem system)
        {
            var eventIds = from id in system.EventIdsOfInterest.Distinct()
                           select id;

            foreach (var eventId in eventIds)
            {
                if (!communicatorsByEventId.ContainsKey(eventId))
                {
                    communicatorsByEventId[eventId] = new List<EventCommunicator>();
                }
                communicatorsByEventId[eventId].Add(system.EventCommunicator);
            }
        }

        /// <summary>
        /// Retrieves pending events from the communicators.
        /// </summary>
        /// <returns>
        /// Number of events processed.
        /// </returns>
        private int RetrievePendingEvents()
        {
            var eventsProcessed = 0;
            foreach (var eventSender in eventSenders)
            {
                while (eventSender.TryGetOutgoingEvent(out var ev))
                {
                    EnqueueEvent(ev);
                    eventsProcessed++;
                }
            }

            return eventsProcessed;
        }

        /// <summary>
        /// Retrieves and enqueues all available events from the IEventAdapters.
        /// </summary>
        /// <returns>
        /// Number of events processed.
        /// </returns>
        private int RetrieveAdaptedEvents()
        {
            var eventsProcessed = 0;
            foreach (IEventAdapter eventAdapter in eventAdapterManager.EventAdapters)
            {
                eventAdapter.PrepareEvents();

                while (eventAdapter.PollEvent(out Event ev))
                {
                    EnqueueEvent(ev);
                    eventsProcessed++;
                }
            }

            return eventsProcessed;
        }

        /// <summary>
        /// Enqueues an event.
        /// </summary>
        /// <param name="ev">Event to enqueue.</param>
        private void EnqueueEvent(Event ev)
        {
            /* Is the event to be dispatched immediately? */
            if (ev.EventTime <= lastUpdateTime)
            {
                /* Dispatch immediately. */
                DispatchImmediateEvent(ev);
            }
            else
            {
                /* Enqueue for later dispatch. */
                futureEventQueue.Add(ev);
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
            ev.EventTime = lastUpdateTime;

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
            while (futureEventQueue.Count > 0 &&
                futureEventQueue.Peek().EventTime <= lastUpdateTime)
            {
                var nextEvent = futureEventQueue.Pop();
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
