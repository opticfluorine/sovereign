/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System.Numerics;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.Persistence;

/// <summary>
///     Event controller for the persistence system.
/// </summary>
public sealed class PersistenceController
{
    /// <summary>
    ///     Requests that the persistence engine load the given entity.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="persistedEntityId">Persisted entity ID.</param>
    public void RetrieveEntity(IEventSender eventSender, ulong persistedEntityId)
    {
        var details = new EntityEventDetails { EntityId = persistedEntityId };
        var ev = new Event(EventId.Server_Persistence_RetrieveEntity, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests that the persistence engine load all entities positioned
    ///     within the given range.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="rangeMin">Minimum extent of range.</param>
    /// <param name="rangeMax">Maximum extent of range.</param>
    public void RetrieveEntitiesInRange(IEventSender eventSender, Vector3 rangeMin,
        Vector3 rangeMax)
    {
        var details = new VectorPairEventDetails { First = rangeMin, Second = rangeMax };
        var ev = new Event(EventId.Server_Persistence_RetrieveEntitiesInRange, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests that the persistence engine loads all entities positioned
    ///     within the given world segment.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void RetrieveWorldSegment(IEventSender eventSender, GridPosition segmentIndex)
    {
        var details = new WorldSegmentEventDetails { SegmentIndex = segmentIndex };
        var ev = new Event(EventId.Server_Persistence_RetrieveWorldSegment, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests that the persistence engine synchronizes with the database.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void Synchronize(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Persistence_Synchronize);
        eventSender.SendEvent(ev);
    }
}