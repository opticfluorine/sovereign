// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.WorldManagement;

public class WorldManagementInternalController
{
    /// <summary>
    ///     Pushes a world segment subscription event to the client.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="worldSegmentIndex">World segment index.</param>
    public void PushSubscribe(IEventSender eventSender, ulong entityId, GridPosition worldSegmentIndex)
    {
        PushSubscriptionEvent(eventSender, entityId, worldSegmentIndex, EventId.Core_WorldManagement_Subscribe);
    }

    /// <summary>
    ///     Pushes a world segment unsubscription event to the client.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="worldSegmentIndex">World segment index.</param>
    public void PushUnsubscribe(IEventSender eventSender, ulong entityId, GridPosition worldSegmentIndex)
    {
        PushSubscriptionEvent(eventSender, entityId, worldSegmentIndex, EventId.Core_WorldManagement_Unsubscribe);
    }

    /// <summary>
    ///     Pushes a batch of entity synchronizations to the client.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Player entity ID.</param>
    /// <param name="definitions">Entity definitions to send.</param>
    public void PushSyncEvent(IEventSender eventSender, ulong entityId, List<EntityDefinition> definitions)
    {
        var details = new EntityDefinitionEventDetails
        {
            PlayerEntityId = entityId,
            EntityDefinitions = definitions
        };
        var ev = new Event(EventId.Core_EntitySync_Sync, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Pushes a non-block entity desynchronization to all subscribers of the given world segment.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void PushDesyncEvent(IEventSender eventSender, ulong entityId, GridPosition segmentIndex)
    {
        var details = new EntityDesyncEventDetails
        {
            EntityId = entityId,
            WorldSegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Core_EntitySync_Desync, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces that an entity has left the given world segment.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="prevSegmentIndex">Segment index of the world segment that was left.</param>
    /// <param name="newSegmentIndex">Segment index of the world segment that was entered.</param>
    public void PushChangeSegment(IEventSender eventSender, ulong entityId, GridPosition prevSegmentIndex,
        GridPosition newSegmentIndex)
    {
        var details = new EntityChangeWorldSegmentEventDetails
        {
            EntityId = entityId,
            PreviousSegmentIndex = prevSegmentIndex,
            NewSegmentIndex = newSegmentIndex
        };
        var ev = new Event(EventId.Core_WorldManagement_EntityLeaveWorldSegment, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Pushes a world segment subscribe/unsubscribe event to the client.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="worldSegmentIndex">World segment index.</param>
    /// <param name="eventId">Event ID to use.</param>
    private void PushSubscriptionEvent(IEventSender eventSender, ulong entityId, GridPosition worldSegmentIndex,
        EventId eventId)
    {
        var details = new WorldSegmentSubscriptionEventDetails
        {
            EntityId = entityId,
            SegmentIndex = worldSegmentIndex
        };
        var ev = new Event(eventId, details);
        eventSender.SendEvent(ev);
    }
}