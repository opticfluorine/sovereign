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
    ///     Requests a resynchronization of the given entity to any subscribers. If the entity is not positioned,
    ///     its parent/ancestors will be checked for the nearest position in the tree; if no position is found,
    ///     there will be no effect.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <remarks>
    ///     The synchronization will not occur earlier than the beginning of the next server tick
    ///     to allow time for any recent component updates to be applied prior to synchronization.
    /// </remarks>
    public void ResyncEntity(IEventSender eventSender, ulong entityId)
    {
        var details = new EntityEventDetails { EntityId = entityId };
        var ev = new Event(EventId.Server_WorldManagement_ResyncEntity, details)
        {
            SyncToTick = true
        };
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests a resynchronization of the given entity, and of all of its descendents,
    ///     to any subscribers. If the entity is not positioned, its parent/ancestors will be checked
    ///     for the nearest position in the tree; if no position is found, there will be no effect.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <remarks>
    ///     The synchronization will not occur earlier than the beginning of the next server tick
    ///     to allow time for any recent component updates to be applied prior to synchronization.
    /// </remarks>
    public void ResyncEntityTree(IEventSender eventSender, ulong entityId)
    {
        var details = new EntityEventDetails { EntityId = entityId };
        var ev = new Event(EventId.Server_WorldManagement_ResyncEntityTree, details)
        {
            SyncToTick = true
        };
        eventSender.SendEvent(ev);
    }
}