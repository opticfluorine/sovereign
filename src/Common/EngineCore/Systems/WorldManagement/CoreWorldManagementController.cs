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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.WorldManagement;

/// <summary>
///     Controller for core world management events used by both client and server.
/// </summary>
public class CoreWorldManagementController
{
    /// <summary>
    ///     Announces that the given world segment has been loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void AnnounceWorldSegmentLoaded(IEventSender eventSender, GridPosition segmentIndex)
    {
        var details = new WorldSegmentEventDetails
        {
            SegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Core_WorldManagement_WorldSegmentLoaded, details);

        // Latch event to the next tick to ensure that entities and components are committed before announcement.
        ev.SyncToTick = true;

        eventSender.SendEvent(ev);
    }
}