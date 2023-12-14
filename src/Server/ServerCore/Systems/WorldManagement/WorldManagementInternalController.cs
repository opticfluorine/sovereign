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

using Sovereign.EngineCore.Components.Indexers;
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