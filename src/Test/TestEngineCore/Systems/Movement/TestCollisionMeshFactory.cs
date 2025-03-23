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

using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.World;
using Xunit;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Unit tests for CollisionMeshFactory.
/// </summary>
public class TestCollisionMeshFactory
{
    private const float ErrorThreshold = 1e-4f;

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength / 2)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, 0, 0, 0)]
    [InlineData(1, -1, 0, 0)]
    public void TestDenseGridSingletons(int segmentX, int segmentY, int segmentZ, int z)
    {
        var denseGrid =
            new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        Array.Fill(denseGrid, true);

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, denseGrid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var expectedSlab = new BoundingBox
        {
            Position = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z },
            Size = new Vector3(WorldConstants.SegmentLength, WorldConstants.SegmentLength,
                1.0f)
        };

        var slabs = factory.MakeBlockwiseSlabs(segmentIndex, z, out var shouldSimplify);

        Assert.Single(slabs);
        Assert.False(shouldSimplify);
        Assert.True(SlabEquals(expectedSlab, slabs[0]));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength / 2)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, 0, 0, 0)]
    [InlineData(1, -1, 0, 0)]
    public void TestDenseGridSimplified(int segmentX, int segmentY, int segmentZ, int z)
    {
        var denseGrid =
            new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        Array.Fill(denseGrid, true);

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, denseGrid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var expectedSlab = new BoundingBox
        {
            Position = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z },
            Size = new Vector3(WorldConstants.SegmentLength, WorldConstants.SegmentLength,
                1.0f)
        };

        var slabs = factory.MakeMergedSlabs(segmentIndex, z);

        Assert.Single(slabs);
        Assert.True(SlabEquals(expectedSlab, slabs[0]));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(0, 0, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(0, 0, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(1, 0, 0, 0, 0, 0)]
    [InlineData(1, 0, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(1, 0, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, 0, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(1, -1, 0, 0, 0, 0)]
    [InlineData(1, -1, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(1, -1, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, -1, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, 0, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(0, 0, 1, 2 * WorldConstants.SegmentLength - 1, 0, 0)]
    public void TestSingleGridSingletons(int segmentX, int segmentY, int segmentZ, int z, int offX, int offY)
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        grid[WorldConstants.SegmentLength * offY + offX] = true;

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var slabs = factory.MakeBlockwiseSlabs(segmentIndex, z, out var shouldSimplify);

        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
        var expected = new BoundingBox
        {
            Position = basePos + new Vector3(offX, offY, 0.0f),
            Size = Vector3.One
        };

        Assert.Single(slabs);
        Assert.False(shouldSimplify);
        Assert.True(SlabEquals(expected, slabs[0]));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(0, 0, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(0, 0, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(0, 0, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(1, 0, 0, 0, 0, 0)]
    [InlineData(1, 0, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(1, 0, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, 0, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(1, -1, 0, 0, 0, 0)]
    [InlineData(1, -1, 0, 0, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(1, -1, 0, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(1, -1, 0, WorldConstants.SegmentLength - 1, 0, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, 0, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, WorldConstants.SegmentLength - 1, 0)]
    [InlineData(0, 0, 1, WorldConstants.SegmentLength, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(0, 0, 1, 2 * WorldConstants.SegmentLength - 1, 0, 0)]
    public void TestSingleGridSimplified(int segmentX, int segmentY, int segmentZ, int z, int offX, int offY)
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        grid[WorldConstants.SegmentLength * offY + offX] = true;

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var slabs = factory.MakeMergedSlabs(segmentIndex, z);

        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
        var expected = new BoundingBox
        {
            Position = basePos + new Vector3(offX, offY, 0.0f),
            Size = Vector3.One
        };

        Assert.Single(slabs);
        Assert.True(SlabEquals(expected, slabs[0]));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 0, 0, WorldConstants.SegmentLength - 1)]
    [InlineData(0, -1, 0, WorldConstants.SegmentLength / 2)]
    [InlineData(7, -4, 3, 0)]
    public void TestPartialGridSingletons(int segmentX, int segmentY, int segmentZ, int z)
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        DrawRect(grid, 0, 0, 6, 5, WorldConstants.SegmentLength); // area 30
        DrawRect(grid, 4, 4, 4, 4, WorldConstants.SegmentLength); // new area (16 - 2) = 14
        DrawRect(grid, 22, 22, 2, 2, WorldConstants.SegmentLength); // area 4

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
        var expected = Enumerable.Range(0, grid.Length)
            .Where(idx => grid[idx])
            .Select(idx => Tuple.Create(idx % WorldConstants.SegmentLength,
                idx / WorldConstants.SegmentLength))
            .Select(c => new BoundingBox
            {
                Position = basePos + new Vector3(c.Item1, c.Item2, 0.0f),
                Size = Vector3.One
            }).ToList();

        var slabs = factory.MakeBlockwiseSlabs(segmentIndex, z, out var shouldSimplify);

        Assert.True(shouldSimplify);
        Assert.Equal(48, slabs.Count);
        Assert.All(slabs, box => Assert.True(ContainsBox(expected, box)));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    public void TestPartialGridSimplified(int segmentX, int segmentY, int segmentZ, int z)
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        DrawRect(grid, 0, 0, 6, 5, WorldConstants.SegmentLength);
        DrawRect(grid, 4, 4, 4, 4, WorldConstants.SegmentLength);
        DrawRect(grid, 8, 9, 2, 2, WorldConstants.SegmentLength);

        var segmentIndex = new GridPosition(segmentX, segmentY, segmentZ);
        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, z, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
        List<BoundingBox> expected =
        [
            new()
            {
                Position = basePos,
                Size = new Vector3(6.0f, 5.0f, 1.0f)
            },
            new()
            {
                Position = basePos + new Vector3(4.0f, 5.0f, 0.0f),
                Size = new Vector3(4.0f, 3.0f, 1.0f)
            },
            new()
            {
                Position = basePos + new Vector3(8.0f, 9.0f, 0.0f),
                Size = new Vector3(2.0f, 2.0f, 1.0f)
            },
            new()
            {
                Position = basePos + new Vector3(6.0f, 4.0f, 0.0f),
                Size = new Vector3(2.0f, 1.0f, 1.0f)
            }
        ];

        var slabs = factory.MakeMergedSlabs(segmentIndex, z);

        Assert.Equal(expected.Count, slabs.Count);
        Assert.All(slabs, box => Assert.True(ContainsBox(expected, box)));
    }

    [Fact]
    public void TestEmptyGridSingletons()
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        var segmentIndex = new GridPosition(0, 0, 0);

        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, 0, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var slabs = factory.MakeBlockwiseSlabs(segmentIndex, 0, out var shouldSimplify);

        Assert.Empty(slabs);
        Assert.False(shouldSimplify);
    }

    [Fact]
    public void TestEmptyGridSimplified()
    {
        var grid = new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
        var segmentIndex = new GridPosition(0, 0, 0);

        var blockServices = MockBlockServicesFactory.GetBuilder()
            .WithBlockPresenceGrid(segmentIndex, 0, grid)
            .Mock.Object;
        var resolver = new WorldSegmentResolver();
        var factory = new CollisionMeshFactory(blockServices, resolver);

        var slabs = factory.MakeMergedSlabs(segmentIndex, 0);

        Assert.Empty(slabs);
    }

    /// <summary>
    ///     Checks whether two collision slabs are equal.
    /// </summary>
    /// <param name="a">First slab.</param>
    /// <param name="b">Second slab.</param>
    private bool SlabEquals(BoundingBox a, BoundingBox b)
    {
        var errorPos = (a.Position - b.Position).LengthSquared();
        var errorSize = (a.Size - b.Size).LengthSquared();

        return errorPos < ErrorThreshold && errorSize < ErrorThreshold;
    }

    /// <summary>
    ///     Draws a rectangle into the grid.
    /// </summary>
    /// <param name="grid">Target grid.</param>
    /// <param name="x0">Starting x (relative to grid).</param>
    /// <param name="y0">Starting y (relative to grid).</param>
    /// <param name="w">Width.</param>
    /// <param name="h">Height.</param>
    /// <param name="stride">Full width of grid.</param>
    private void DrawRect(bool[] grid, int x0, int y0, int w, int h, uint stride)
    {
        for (var y = y0; y < y0 + h; ++y)
        for (var x = x0; x < x0 + w; ++x)
        {
            var idx = stride * y + x;
            grid[idx] = true;
        }
    }

    /// <summary>
    ///     Checks whether a collection contains the given bounding box.
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="toFind">Box to find.</param>
    /// <returns>true if found, false otherwise.</returns>
    private bool ContainsBox(IEnumerable<BoundingBox> collection, BoundingBox toFind)
    {
        return collection.Any(box => SlabEquals(box, toFind));
    }
}