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

using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.World;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.Systems.Combat;

/// <summary>
///     Decides whether player-versus-player damage is permitted between two positions.
/// </summary>
public sealed class PvpGate
{
    private readonly SegmentFlagManager segmentFlagManager;
    private readonly WorldSegmentResolver segmentResolver;

    public PvpGate(SegmentFlagManager segmentFlagManager, WorldSegmentResolver segmentResolver)
    {
        this.segmentFlagManager = segmentFlagManager;
        this.segmentResolver = segmentResolver;
    }

    /// <summary>
    ///     Checks whether player-initiated damage from the actor position to the target position is
    ///     allowed, which requires the Pvp flag on both positions' world segments.
    /// </summary>
    /// <param name="actorPosition">Position of the attacking player.</param>
    /// <param name="targetPosition">Position of the target player.</param>
    /// <returns>true if the damage is allowed, false otherwise.</returns>
    public bool IsPvpDamageAllowed(Vector3 actorPosition, Vector3 targetPosition)
    {
        var actorSegment = segmentResolver.GetWorldSegmentForPosition(actorPosition);
        var targetSegment = segmentResolver.GetWorldSegmentForPosition(targetPosition);

        return (segmentFlagManager.GetFlags(actorSegment) & WorldSegmentFlag.Pvp) != 0
               && (segmentFlagManager.GetFlags(targetSegment) & WorldSegmentFlag.Pvp) != 0;
    }
}
