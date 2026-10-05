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
///     Public API for the combat system.
/// </summary>
public class CombatController
{
    /// <summary>
    ///     Requests that the given actor perform an attack.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="actorId">Entity ID of the attacking actor.</param>
    public void Attack(IEventSender eventSender, ulong actorId)
    {
        var details = new AttackEventDetails
        {
            ActorId = actorId
        };
        var ev = new Event(EventId.Server_Combat_Attack, details);
        eventSender.SendEvent(ev);
    }
}
