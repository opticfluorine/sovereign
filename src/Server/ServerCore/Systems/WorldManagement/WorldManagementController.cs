// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Public asynchronous API for the WorldManagement system.
/// </summary>
public class WorldManagementController
{
    /// <summary>
    ///     Requests a resynchronization of the given positioned entity, and all of its descendents,
    ///     to any subscribers.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Positioned entity ID.</param>
    /// <remarks>
    ///     The synchronization will not occur earlier than the beginning of the next server tick
    ///     to allow time for any recent component updates to be applied prior to synchronization.
    /// </remarks>
    public void ResyncPositionedEntity(IEventSender eventSender, ulong entityId)
    {
        var details = new EntityEventDetails { EntityId = entityId };
        var ev = new Event(EventId.Server_WorldManagement_ResyncPositionedEntity, details);
        ev.SyncToTick = true;
        eventSender.SendEvent(ev);
    }
}