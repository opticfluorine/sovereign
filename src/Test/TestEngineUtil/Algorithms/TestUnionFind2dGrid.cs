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

using System.Collections.Generic;
using Sovereign.EngineUtil.Algorithms;
using Xunit;

namespace TestEngineUtil.Algorithms;

public class TestUnionFind2dGrid
{
    [Fact]
    public void AddVertices_And_TryGetComponent_Works()
    {
        var uf = new UnionFind2dGrid();
        var v1 = (0, 0);
        var v2 = (0, 1);
        var v3 = (1, 0);
        var v4 = (2, 2);
        uf.AddVertices(new[] { v1, v2, v3, v4 });
        Assert.True(uf.TryGetComponent(v1, out var c1));
        Assert.True(uf.TryGetComponent(v2, out var c2));
        Assert.True(uf.TryGetComponent(v3, out var c3));
        Assert.True(uf.TryGetComponent(v4, out var c4));
        // v1, v2, v3 should be in the same component
        Assert.Equal(c1, c2);
        Assert.Equal(c1, c3);
        // v4 should be in a different component
        Assert.NotEqual(c1, c4);
    }

    [Fact]
    public void RemoveVertices_SplitsComponents()
    {
        var uf = new UnionFind2dGrid();
        var v1 = (0, 0);
        var v2 = (0, 1);
        var v3 = (0, 2);
        uf.AddVertices(new[] { v1, v2, v3 });
        Assert.True(uf.TryGetComponent(v1, out var c1));
        Assert.True(uf.TryGetComponent(v2, out var c2));
        Assert.True(uf.TryGetComponent(v3, out var c3));
        Assert.Equal(c1, c2);
        Assert.Equal(c1, c3);
        // Remove the middle vertex
        uf.RemoveVertices(new[] { v2 });
        Assert.True(uf.TryGetComponent(v1, out var c1a));
        Assert.True(uf.TryGetComponent(v3, out var c3a));
        Assert.NotEqual(c1a, c3a);
        Assert.False(uf.TryGetComponent(v2, out _));
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var uf = new UnionFind2dGrid();
        var v1 = (0, 0);
        var v2 = (0, 1);
        uf.AddVertices(new[] { v1, v2 });
        var clone = uf.Clone();
        Assert.True(clone.TryGetComponent(v1, out var c1));
        Assert.True(clone.TryGetComponent(v2, out var c2));
        Assert.Equal(c1, c2);
        // Mutate original, clone should not change
        uf.RemoveVertices(new[] { v2 });
        Assert.True(clone.TryGetComponent(v2, out var c2clone));
        Assert.Equal(c1, c2clone);
    }

    [Fact]
    public void AddVertices_DoesNotDuplicate()
    {
        var uf = new UnionFind2dGrid();
        var v = (5, 5);
        uf.AddVertices(new[] { v });
        uf.AddVertices(new[] { v }); // Should not throw or change state
        Assert.True(uf.TryGetComponent(v, out _));
    }

    [Fact]
    public void TryGetComponent_ReturnsFalse_ForMissingVertex()
    {
        var uf = new UnionFind2dGrid();
        Assert.False(uf.TryGetComponent((100, 100), out _));
    }

    [Fact]
    public void RemoveVertices_SplitsIntoTwoLargerComponents_FloodFillRelabelsCorrectly()
    {
        // Create a 3x3 grid:
        // (0,0) (0,1) (0,2)
        // (1,0) (1,1) (1,2)
        // (2,0) (2,1) (2,2)
        var uf = new UnionFind2dGrid();
        var all = new List<(int, int)>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                all.Add((x, y));
        uf.AddVertices(all);
        // Remove the center column (x=1)
        var toRemove = new[] { (1, 0), (1, 1), (1, 2) };
        uf.RemoveVertices(toRemove);
        // Now, left and right columns should be two separate components
        var left = new[] { (0, 0), (0, 1), (0, 2) };
        var right = new[] { (2, 0), (2, 1), (2, 2) };
        // All left should be in the same component
        Assert.True(uf.TryGetComponent((0, 0), out var leftId));
        foreach (var v in left)
            Assert.True(uf.TryGetComponent(v, out var id) && id == leftId);
        // All right should be in the same component, but different from left
        Assert.True(uf.TryGetComponent((2, 0), out var rightId));
        foreach (var v in right)
            Assert.True(uf.TryGetComponent(v, out var id) && id == rightId);
        Assert.NotEqual(leftId, rightId);
        // Removed vertices should not be present
        foreach (var v in toRemove)
            Assert.False(uf.TryGetComponent(v, out _));
    }
}