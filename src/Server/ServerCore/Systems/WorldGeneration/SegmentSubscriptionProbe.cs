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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     ISegmentSubscriptionProbe backed by the world management services API.
/// </summary>
public sealed class SegmentSubscriptionProbe : ISegmentSubscriptionProbe
{
    private readonly WorldManagementServices worldManagementServices;

    public SegmentSubscriptionProbe(WorldManagementServices worldManagementServices)
    {
        this.worldManagementServices = worldManagementServices;
    }

    /// <summary>
    ///     Gets the players who are currently subscribed to the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Set of players (possibly empty) subscribed to the world segment.</returns>
    public IReadOnlySet<ulong> GetSubscribersForWorldSegment(GridPosition segmentIndex)
    {
        return worldManagementServices.GetPlayersSubscribedToWorldSegment(segmentIndex);
    }
}
