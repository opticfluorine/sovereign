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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Internal controller API for Persistence system.
/// </summary>
public class PersistenceInternalController
{
    private readonly ILogger<PersistenceInternalController> logger;
    private readonly NameComponentCollection names;

    public PersistenceInternalController(NameComponentCollection names, ILogger<PersistenceInternalController> logger)
    {
        this.names = names;
        this.logger = logger;
    }

    /// <summary>
    ///     Announces that synchronization is complete.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void CompleteSync(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Persistence_SynchronizeComplete);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces that the data load is complete for a player entering the world.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void PlayerEnteredWorld(IEventSender eventSender, ulong playerEntityId)
    {
        var details = new EntityEventDetails
        {
            EntityId = playerEntityId
        };
        var ev = new Event(EventId.Server_Persistence_PlayerEnteredWorld, details);
        ev.SyncToTick = true;
        eventSender.SendEvent(ev);
    }
}