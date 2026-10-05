// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.EngineCore.Systems.Combat;

/// <summary>
///     Internal controller API for server-internal combat events.
/// </summary>
public class CombatInternalController
{
    /// <summary>
    ///     Announces that an entity has been damaged by an attacker.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="victimEntityId">Entity ID of the damaged entity.</param>
    /// <param name="attackerEntityId">Entity ID of the attacker.</param>
    public void DamagedBy(IEventSender eventSender, ulong victimEntityId, ulong attackerEntityId)
    {
        var details = new DamagedByEventDetails
        {
            VictimEntityId = victimEntityId,
            AttackerEntityId = attackerEntityId
        };
        var ev = new Event(EventId.Server_Combat_DamagedBy, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces that a player has been killed by another entity.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="victimEntityId">Entity ID of the slain player.</param>
    /// <param name="killerEntityId">Entity ID of the killer, or 0 if unknown.</param>
    public void PlayerKilled(IEventSender eventSender, ulong victimEntityId, ulong killerEntityId)
    {
        var details = new PlayerKilledEventDetails
        {
            VictimEntityId = victimEntityId,
            KillerEntityId = killerEntityId
        };
        var ev = new Event(EventId.Server_Combat_PlayerKilled, details);
        eventSender.SendEvent(ev);
    }
}
