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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Vitals;

/// <summary>
///     Public API for the vitals system.
/// </summary>
public class VitalsController
{
    /// <summary>
    ///     Kills the given entity.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    public void Kill(IEventSender eventSender, ulong entityId)
    {
        var details = new EntityEventDetails
        {
            EntityId = entityId
        };
        var ev = new Event(EventId.Core_Vitals_Kill, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Changes the value of a vital of the given entity.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="vital">Vital to change.</param>
    /// <param name="changeAmount">Amount by which to change the vital's value.</param>
    public void ChangeVitals(IEventSender eventSender, ulong entityId, VitalType vital, int changeAmount)
    {
        var details = new ChangeVitalsEventDetails
        {
            EntityId = entityId,
            Vital = vital,
            ChangeAmount = changeAmount
        };
        var ev = new Event(EventId.Core_Vitals_ChangeVitals, details);
        eventSender.SendEvent(ev);
    }
}
