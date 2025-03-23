// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Internal controller class for the Block system.
/// </summary>
public class BlockInternalController
{
    private readonly IEventSender eventSender;

    public BlockInternalController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Announces that the block presence grid has been updated for a given world segment and Z plane.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z coordinate of the Z plane.</param>
    public void AnnounceBlockPresenceGridUpdated(GridPosition segmentIndex, int z)
    {
        var details = new BlockPresenceGridUpdatedEventDetails
        {
            WorldSegmentIndex = segmentIndex,
            Z = z
        };
        var ev = new Event(EventId.Core_Block_GridUpdated, details);
        eventSender.SendEvent(ev);
    }
}