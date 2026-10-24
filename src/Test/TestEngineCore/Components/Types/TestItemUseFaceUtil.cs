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

using Xunit;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Unit tests for <see cref="ItemUseFaceUtil" />.
/// </summary>
public class TestItemUseFaceUtil
{
    [Fact]
    public void TryGetAdjacentCell_TopFace_ReturnsCellAbove()
    {
        var blockPosition = new GridPosition(2, 3, 4);

        var found = ItemUseFaceUtil.TryGetAdjacentCell(blockPosition, ItemUseFace.BlockTopFace, out var cell);

        Assert.True(found);
        Assert.Equal(new GridPosition(2, 3, 5), cell);
    }

    [Fact]
    public void TryGetAdjacentCell_FrontFace_ReturnsCellInFront()
    {
        var blockPosition = new GridPosition(2, 3, 4);

        var found = ItemUseFaceUtil.TryGetAdjacentCell(blockPosition, ItemUseFace.BlockFrontFace, out var cell);

        Assert.True(found);
        Assert.Equal(new GridPosition(2, 2, 4), cell);
    }

    [Fact]
    public void TryGetAdjacentCell_NoFace_ReturnsFalse()
    {
        var found = ItemUseFaceUtil.TryGetAdjacentCell(new GridPosition(2, 3, 4), ItemUseFace.None, out _);

        Assert.False(found);
    }
}
