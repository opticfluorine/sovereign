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
using Xunit;

namespace Sovereign.ServerCore.Systems.Combat;

/// <summary>
///     Unit tests for PvpGate.
/// </summary>
public class TestPvpGate
{
    private readonly SegmentFlagManager segmentFlags = new();
    private readonly PvpGate gate;

    public TestPvpGate()
    {
        gate = new PvpGate(segmentFlags, new WorldSegmentResolver());
    }

    [Fact]
    public void BothSegmentsFlagged_Allows()
    {
        var actor = SegmentPosition(0);
        var target = SegmentPosition(1);
        segmentFlags.SetFlagsInCache(new GridPosition(0, 0, 0), WorldSegmentFlag.Pvp);
        segmentFlags.SetFlagsInCache(new GridPosition(1, 0, 0), WorldSegmentFlag.Pvp);

        Assert.True(gate.IsPvpDamageAllowed(actor, target));
    }

    [Fact]
    public void ActorFlagged_TargetNot_Blocks()
    {
        var actor = SegmentPosition(0);
        var target = SegmentPosition(1);
        segmentFlags.SetFlagsInCache(new GridPosition(0, 0, 0), WorldSegmentFlag.Pvp);

        Assert.False(gate.IsPvpDamageAllowed(actor, target));
    }

    [Fact]
    public void TargetFlagged_ActorNot_Blocks()
    {
        var actor = SegmentPosition(0);
        var target = SegmentPosition(1);
        segmentFlags.SetFlagsInCache(new GridPosition(1, 0, 0), WorldSegmentFlag.Pvp);

        Assert.False(gate.IsPvpDamageAllowed(actor, target));
    }

    [Fact]
    public void NeitherSegmentFlagged_Blocks()
    {
        Assert.False(gate.IsPvpDamageAllowed(SegmentPosition(0), SegmentPosition(1)));
    }

    [Fact]
    public void SameSegmentFlagged_Allows()
    {
        var position = SegmentPosition(0);
        segmentFlags.SetFlagsInCache(new GridPosition(0, 0, 0), WorldSegmentFlag.Pvp);

        Assert.True(gate.IsPvpDamageAllowed(position, position));
    }

    [Fact]
    public void SameSegmentUnflagged_Blocks()
    {
        var position = SegmentPosition(2);
        Assert.False(gate.IsPvpDamageAllowed(position, position));
    }

    /// <summary>
    ///     Gets a position within the given segment X index along the X axis.
    /// </summary>
    /// <param name="segmentX">Segment X index.</param>
    /// <returns>Position within the segment.</returns>
    private static Vector3 SegmentPosition(int segmentX)
    {
        return new Vector3(segmentX * 32 + 1, 1, 1);
    }
}
