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

using MessagePack;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Interface implemented by event classes.
/// </summary>
[MessagePackObject]
public class Event
{
    /// <summary>
    ///     Time used to indicate that the event should be dispatched immediately.
    /// </summary>
    public const ulong Immediate = 0;

    /// <summary>
    ///     Default constructor, needed for Protobuf compatibility.
    /// </summary>
    public Event()
    {
    }

    public Event(EventId eventId, ulong eventTime = Immediate)
        : this(eventId, null, eventTime)
    {
    }

    /// <summary>
    ///     Creates a new Event object.
    /// </summary>
    /// <param name="eventId">Event type ID.</param>
    /// <param name="details">Associated event details.</param>
    /// <param name="eventTime">
    ///     System time (us) when this event should be dispatched.
    /// </param>
    public Event(EventId eventId, IEventDetails? details,
        ulong eventTime = Immediate, bool local = true)
    {
        EventId = eventId;
        EventDetails = details;
        EventTime = eventTime;
        Local = local;
    }

    /// <summary>
    ///     Shallow copy constructor that updates the locality flag.
    /// </summary>
    /// <param name="other">Event to be copied.</param>
    /// <param name="local">New value for Local.</param>
    /// <remarks>
    ///     This constructor performs a shallow copy only; the EventDetails
    ///     object, if any, is not copied.
    /// </remarks>
    public Event(Event other, bool local = false)
    {
        EventId = other.EventId;
        EventDetails = other.EventDetails;
        EventTime = other.EventTime;
        Local = local;
    }

    /// <summary>
    ///     Unique identifier for the event type.
    /// </summary>
    [Key(0)]
    public EventId EventId { get; set; }

    /// <summary>
    ///     System time at which the event becomes valid, in microseconds.
    /// </summary>
    /// <remarks>
    ///     This field is not sent over the network as timed events will not
    ///     be relayed until their trigger time is reached.
    /// </remarks>
    [IgnoreMember]
    public ulong EventTime { get; set; }

    /// <summary>
    ///     System time at which the event was dispatched by the main event loop, in microseconds.
    /// </summary>
    /// <remarks>
    ///     This field is intended for diagnostics and debugging purposes only.
    ///     It is only set if event logging is enabled.
    ///     It is not sent over the network.
    /// </remarks>
    [IgnoreMember]
    public ulong DispatchTime { get; set; }

    /// <summary>
    ///     Flag indicating that the event originated locally
    ///     (i.e. not from a remote client or server).
    /// </summary>
    /// <remarks>
    ///     This field is not sent over the network as the value of the
    ///     locality flag depends on the execution context.
    /// </remarks>
    [IgnoreMember]
    public bool Local { get; set; }

    /// <summary>
    ///     If true, the vent will be sent at the start of the first fully eligible tick.
    /// </summary>
    [IgnoreMember]
    public bool SyncToTick { get; set; }

    /// <summary>
    ///     Connection ID that this originated from. Only meaningful if Local is false.
    /// </summary>
    [IgnoreMember]
    public int FromConnectionId { get; set; }

    /// <summary>
    ///     Details associated with the event.
    /// </summary>
    [Key(1)]
    public IEventDetails? EventDetails { get; set; }
}