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

using System.Linq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.ServerCore.Systems.WorldManagement;
using Xunit;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Unit tests for SegmentFlagManager.
/// </summary>
public class TestSegmentFlagManager
{
    private readonly SegmentFlagManager manager = new();

    [Fact]
    public void GetFlags_MissingSegment_ReturnsNone()
    {
        Assert.Equal(WorldSegmentFlag.None, manager.GetFlags(new GridPosition(1, 2, 3)));
    }

    [Fact]
    public void SetFlagsInCache_InsertsAndReadsBack()
    {
        var segment = new GridPosition(4, 5, 6);
        manager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);
        Assert.Equal(WorldSegmentFlag.Pvp, manager.GetFlags(segment));
    }

    [Fact]
    public void SetFlagsInCache_OverwritesExistingValue()
    {
        var segment = new GridPosition(7, 8, 9);
        manager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);
        manager.SetFlagsInCache(segment, WorldSegmentFlag.None);
        Assert.Equal(WorldSegmentFlag.None, manager.GetFlags(segment));

        manager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);
        Assert.Equal(WorldSegmentFlag.Pvp, manager.GetFlags(segment));
    }

    [Fact]
    public void SetFlagsInCache_ZeroRemovesEntry()
    {
        var segment = new GridPosition(10, 11, 12);
        manager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);
        manager.SetFlagsInCache(segment, WorldSegmentFlag.None);

        Assert.DoesNotContain(segment, manager.GetFlagsInBox(segment, segment).Select(entry => entry.segment));
    }

    [Fact]
    public void GetFlagsInBox_IsInclusiveOnAllFaces()
    {
        var inner = new GridPosition(0, 0, 0);
        var corner = new GridPosition(2, 2, 2);
        var outside = new GridPosition(3, 3, 3);
        manager.SetFlagsInCache(inner, WorldSegmentFlag.Pvp);
        manager.SetFlagsInCache(corner, WorldSegmentFlag.Pvp);
        manager.SetFlagsInCache(outside, WorldSegmentFlag.Pvp);

        var inBox = manager.GetFlagsInBox(new GridPosition(0, 0, 0), new GridPosition(2, 2, 2))
            .Select(entry => entry.segment)
            .ToHashSet();

        Assert.Contains(inner, inBox);
        Assert.Contains(corner, inBox);
        Assert.DoesNotContain(outside, inBox);
    }

    [Fact]
    public void GetFlagsInBox_ExcludesSegmentsOutsideNegativeBounds()
    {
        var inside = new GridPosition(-1, -1, -1);
        var outside = new GridPosition(-2, -2, -2);
        manager.SetFlagsInCache(inside, WorldSegmentFlag.Pvp);
        manager.SetFlagsInCache(outside, WorldSegmentFlag.Pvp);

        var inBox = manager.GetFlagsInBox(new GridPosition(-1, -1, -1), new GridPosition(1, 1, 1))
            .Select(entry => entry.segment)
            .ToHashSet();

        Assert.Contains(inside, inBox);
        Assert.DoesNotContain(outside, inBox);
    }
}
