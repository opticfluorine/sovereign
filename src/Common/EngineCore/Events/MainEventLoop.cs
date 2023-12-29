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

using System.Collections.Generic;
using System.Linq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Main event loop provided by the event system.
/// </summary>
public class MainEventLoop : IEventLoop
{
    /// <summary>
    ///     Default size of the future event queue.
    /// </summary>
    private const int QUEUE_SIZE = 4096;

    /// <summary>
    ///     Event communicators listening to each event ID.
    /// </summary>
    private readonly Dictionary<EventId, List<EventCommunicator>> communicatorsByEventId = new();

    /// <summary>
    ///     Component manager.
    /// </summary>
    private readonly ComponentManager componentManager;

    /// <summary>
    ///     Event adapter manager.
    /// </summary>
    private readonly EventAdapterManager eventAdapterManager;

    /// <summary>
    ///     The collection of event senders to listen on.
    /// </summary>
    private readonly HashSet<IEventSender> eventSenders = new();

    /// <summary>
    ///     Priority queue of future events ordered by dispatch time.
    /// </summary>
    private readonly IHeap<Event> futureEventQueue
        = new BinaryHeap<Event>(QUEUE_SIZE, new EventTimeComparer());

    /// <summary>
    ///     Queue of tick-synced events waiting for dispatch at the start of the next tick.
    /// </summary>
    private readonly Queue<Event> heldTickSyncedEvents = new();

    /// <summary>
    ///     The collection of systems known to the event loop.
    /// </summary>
    private readonly HashSet<ISystem> systems = new();

    /// <summary>
    ///     The system time of the last update step (microseconds).
    /// </summary>
    private ulong lastUpdateTime;

    public MainEventLoop(ComponentManager componentManager,
        EventAdapterManager eventAdapterManager)
    {
        this.componentManager = componentManager;
        this.eventAdapterManager = eventAdapterManager;
    }

    public bool Terminated { get; private set; }

    public int PumpEventLoop()
    {
        /* Retrieve pending events from the communicators. */
        var eventsProcessed = RetrievePendingEvents();

        /* Retrieve events from the event adapters. */
        eventsProcessed += RetrieveAdaptedEvents();

        return eventsProcessed;
    }

    /// <summary>
    ///     Updates the system time to the next tick if needed.
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
    ///     Emits the tick event at the beginning of a tick.
    /// </summary>
    private void EmitTickEvent()
    {
        // Emit tick.
        var ev = new Event(EventId.Core_Tick);
        EnqueueEvent(ev);

        // Emit per-tick performance events.
        var latencyEv = new Event(EventId.Core_Performance_EventLatencyTest,
            new TimeEventDetails { SystemTime = lastUpdateTime });
        EnqueueEvent(latencyEv);

        // Emit any tick-synced events that are currently held.
        while (heldTickSyncedEvents.TryDequeue(out var heldEv)) DispatchImmediateEvent(heldEv);
    }

    /// <summary>
    ///     Builds the communicator tables.
    /// </summary>
    /// <param name="system">System.</param>
    private void UpdateCommunicatorTables(ISystem system)
    {
        var eventIds = from id in system.EventIdsOfInterest.Distinct()
            select id;

        foreach (var eventId in eventIds)
        {
            if (!communicatorsByEventId.ContainsKey(eventId))
                communicatorsByEventId[eventId] = new List<EventCommunicator>();
            communicatorsByEventId[eventId].Add(system.EventCommunicator);
        }
    }

    /// <summary>
    ///     Retrieves pending events from the communicators.
    /// </summary>
    /// <returns>
    ///     Number of events processed.
    /// </returns>
    private int RetrievePendingEvents()
    {
        var eventsProcessed = 0;
        foreach (var eventSender in eventSenders)
            while (eventSender.TryGetOutgoingEvent(out var ev))
            {
                if (ev != null) EnqueueEvent(ev);
                eventsProcessed++;
            }

        return eventsProcessed;
    }

    /// <summary>
    ///     Retrieves and enqueues all available events from the IEventAdapters.
    /// </summary>
    /// <returns>
    ///     Number of events processed.
    /// </returns>
    private int RetrieveAdaptedEvents()
    {
        var eventsProcessed = 0;
        foreach (var eventAdapter in eventAdapterManager.EventAdapters)
        {
            eventAdapter.PrepareEvents();

            while (eventAdapter.PollEvent(out var ev))
            {
                if (ev != null) EnqueueEvent(ev);
                eventsProcessed++;
            }
        }

        return eventsProcessed;
    }

    /// <summary>
    ///     Enqueues an event.
    /// </summary>
    /// <param name="ev">Event to enqueue.</param>
    private void EnqueueEvent(Event ev)
    {
        /* Is the event to be dispatched immediately? */
        if (ev.EventTime <= lastUpdateTime)
            /* Dispatch immediately. */
            DispatchImmediateEvent(ev);
        else
            /* Enqueue for later dispatch. */
            futureEventQueue.Add(ev);
    }

    /// <summary>
    ///     Immediately dispatches the given event, or enqueues it for immediate dispatch
    ///     at the start of the next tick if the event is synced to tick.
    /// </summary>
    /// <param name="ev">Event to be immediately dispatched.</param>
    private void DispatchImmediateEvent(Event ev)
    {
        // Hold the event until the next tick if it is synced.
        if (ev.SyncToTick)
        {
            // Clear the flag so that it doesn't get held on the next pass.
            ev.SyncToTick = false;
            heldTickSyncedEvents.Enqueue(ev);
            return;
        }

        // Event is ready to go, process immediate dispatch.
        var eventId = ev.EventId;

        /* Handle Core_Quit events specially. */
        if (eventId == EventId.Core_Quit) Terminated = true;

        /* Set the event time to the current tick time. */
        ev.EventTime = lastUpdateTime;

        /* Dispatch to all interested communicators, if any.. */
        if (communicatorsByEventId.TryGetValue(eventId, out var communicators))
            foreach (var comm in communicators)
                comm.SendEventToSystem(ev);
    }

    /// <summary>
    ///     Dispatches all enqueued events that are scheduled for delivery
    ///     no later than the present system time.
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
    ///     Comparer that sorts events by their event time.
    /// </summary>
    private class EventTimeComparer : Comparer<Event>
    {
        public override int Compare(Event? x, Event? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            return y == null ? 1 : x.EventTime.CompareTo(y.EventTime);
        }
    }
}