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

using System;
using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Responsible for creating collision meshes from blocks.
/// </summary>
public class CollisionMeshFactory
{
    /// <summary>
    ///     Maximum iteration count for simplification.
    /// </summary>
    private const int MaxSimplifyIters = 8;

    private readonly IBlockServices blockServices;
    private readonly WorldSegmentResolver resolver;

    public CollisionMeshFactory(IBlockServices blockServices, WorldSegmentResolver resolver)
    {
        this.blockServices = blockServices;
        this.resolver = resolver;
    }

    /// <summary>
    ///     Creates a set of collision slabs in a blockwise fashion (one slab per block)
    ///     without any simplification. This mesh is very fast to create but requires many
    ///     collision tests at runtime. It is suitable as an initial collision mesh upon segment
    ///     load to begin processing physics as quickly as possible, but should be replaced with
    ///     a simplified mesh when possible.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z plane.</param>
    /// <param name="shouldSimplify">Flag indicating whether further simplification may be possible.</param>
    /// <returns>Collision mesh.</returns>
    public List<BoundingBox> MakeBlockwiseSlabs(GridPosition segmentIndex, int z, out bool shouldSimplify)
    {
        shouldSimplify = false;
        var slabs = new List<BoundingBox>();
        if (!blockServices.TryGetBlockPresenceGrid(segmentIndex, z, out var grid)
            || TrySimpleCases(slabs, grid, segmentIndex, z)) return slabs;

        shouldSimplify = true;
        AddSingletons(slabs, grid, segmentIndex, z);

        return slabs;
    }

    /// <summary>
    ///     Creates a set of collision slabs for the given world segment and z plane with
    ///     one or more simplification passes. This mesh is slower to construct but faster for
    ///     performing physics calculations as it requires less collision tests.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z plane.</param>
    /// <returns>Collision mesh.</returns>
    public List<BoundingBox> MakeMergedSlabs(GridPosition segmentIndex, int z)
    {
        var slabs = new List<BoundingBox>();
        if (!blockServices.TryGetBlockPresenceGrid(segmentIndex, z, out var grid)
            || TrySimpleCases(slabs, grid, segmentIndex, z)) return slabs;

        AddSimplified(slabs, grid, segmentIndex, z, MaxSimplifyIters);

        return slabs;
    }

    /// <summary>
    ///     Tries to quickly handle mesh generation in special cases with simple solutions.
    /// </summary>
    /// <param name="slabs">List of slabs to populate.</param>
    /// <param name="grid">Block presence grid.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z plane.</param>
    /// <returns>true if a special case was utilized, false otherwise.</returns>
    private bool TrySimpleCases(List<BoundingBox> slabs, BlockPresenceGrid grid, GridPosition segmentIndex, int z)
    {
        // Simple case 1: Layer is densely packed, in which case the mesh can be built with a single
        // slab spanning the world segment.
        if (grid.Count ==
            WorldConstants.SegmentLength * WorldConstants.SegmentLength)
        {
            var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
            slabs.Add(new BoundingBox
            {
                Position = basePos,
                Size = new Vector3(WorldConstants.SegmentLength,
                    WorldConstants.SegmentLength, 1.0f)
            });
            return true;
        }

        // Simple case 2: Layer is a single block, in which case the mesh is a single slab for that block.
        if (grid.Count == 1)
        {
            AddSingletons(slabs, grid, segmentIndex, z);
            return true;
        }

        // Simple case 3: Layer is empty.
        if (grid.Count == 0) return true;

        return false;
    }

    /// <summary>
    ///     Adds a slab for every block in the grid.
    /// </summary>
    /// <param name="slabs">Slab list to update.</param>
    /// <param name="grid">Block presence grid.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z plane.</param>
    private void AddSingletons(List<BoundingBox> slabs, BlockPresenceGrid grid, GridPosition segmentIndex, int z)
    {
        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };

        AddSingletonsFromSpan(slabs, new ReadOnlySpan<bool>(grid.Grid), basePos, grid.Count);
    }

    /// <summary>
    ///     Adds a slab for every block in the grid, optionally up to a maximum number of blocks.
    /// </summary>
    /// <param name="slabs">Slab list to update.</param>
    /// <param name="grid">Block presence grid.</param>
    /// <param name="basePos">Base ppsition of z plane.</param>
    /// <param name="maxBlocks">Maximum number of blocks to process, or -1 for no limit.</param>
    private static void AddSingletonsFromSpan(List<BoundingBox> slabs, ReadOnlySpan<bool> grid, Vector3 basePos,
        int maxBlocks = -1)
    {
        var found = 0;
        for (var i = 0; i < WorldConstants.SegmentLength; ++i)
        for (var j = 0; j < WorldConstants.SegmentLength; ++j)
        {
            var idx = j * (int)WorldConstants.SegmentLength + i;
            if (!grid[idx]) continue;

            var offset = new Vector3(i, j, 0);
            slabs.Add(new BoundingBox
            {
                Position = basePos + offset,
                Size = Vector3.One
            });

            found++;
            if (found == maxBlocks) return;
        }
    }

    /// <summary>
    ///     Adds simplified slabs by greedily taking largest rectangles in the grid.
    /// </summary>
    /// <param name="slabs">List of collision slabs to populate.</param>
    /// <param name="grid">Block presence grid.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z plane.</param>
    /// <param name="maxIter">Maximum number of simplification passes.</param>
    private void AddSimplified(List<BoundingBox> slabs, BlockPresenceGrid grid, GridPosition segmentIndex, int z,
        int maxIter)
    {
        // Setup various buffers used by the calculation.
        // These are all stackalloc'd to limit GC pressure.
        Span<int> heights = stackalloc int[(int)WorldConstants.SegmentLength];
        Span<bool> tempGrid = stackalloc bool[grid.Grid.Length];
        var srcSpan = new ReadOnlySpan<bool>(grid.Grid);
        srcSpan.CopyTo(tempGrid);

        // The general idea of the following is to sweep up the y axis of the plane, identifying the
        // largest rectangle in the "histogram" formed by the grid from the minimum y through the
        // current row. The histogram is updated per row using a dynamic programming approach.
        // The largest area from all such histograms is identified, then pulled out as a single
        // collision slab. This is repeated up to maxIter times or until the maximum area is <= 1.
        var basePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1 with { Z = z };
        var maxArea = 0;
        var maxAreaI0 = 0;
        var maxAreaWidth = 0;
        var maxAreaJ0 = 0;
        var maxAreaHeight = 0;

        var passes = 0;
        while (passes < maxIter)
        {
            passes++;

            heights.Clear();
            maxArea = 0;

            // Step 1: Find largest rectangle in the grid.
            for (var j = 0; j < WorldConstants.SegmentLength; ++j)
            {
                UpdateHeights(j, tempGrid, heights);
                FindLargestRect(heights, ref maxArea, j, ref maxAreaWidth, ref maxAreaHeight, ref maxAreaI0,
                    ref maxAreaJ0);
            }

            // Step 2: Pull out the largest rectangle as a single collision slab.
            if (maxArea == 0) break;
            ExtractRect(slabs, basePos, maxAreaI0, maxAreaJ0, maxAreaWidth, maxAreaHeight, tempGrid);

            // End processing if no more rectangles can be formed.
            if (maxArea < 2) break;
        }

        // There may be some blocks which were not simplified, so generate a separate slab for each.
        if (maxArea > 0)
            AddSingletonsFromSpan(slabs, tempGrid, basePos);
    }

    /// <summary>
    ///     Updates the height histogram for the next row.
    /// </summary>
    /// <param name="j">New row index.</param>
    /// <param name="tempGrid">Working copy of block presence grid.</param>
    /// <param name="heights">Height histogram to update.</param>
    private static void UpdateHeights(int j, Span<bool> tempGrid, Span<int> heights)
    {
        for (var i = 0; i < WorldConstants.SegmentLength; ++i)
        {
            var idx = (int)WorldConstants.SegmentLength * j + i;
            if (tempGrid[idx]) heights[i]++;
            else heights[i] = 0;
        }
    }

    /// <summary>
    ///     Finds the largest rectangle in the current height histogram.
    /// </summary>
    /// <param name="heights">Current height histogram.</param>
    /// <param name="maxArea">Current maximum area (may be updated).</param>
    /// <param name="j">Current row index.</param>
    /// <param name="maxAreaWidth">Current width of maximum rectangle (may be updated).</param>
    /// <param name="maxAreaHeight">Current height of maximum rectangle (may be updated).</param>
    /// <param name="maxAreaI0">Current starting column index of maximum rectangle (may be updated).</param>
    /// <param name="maxAreaJ0">Current starting row index of maximum rectangle (may be updated).</param>
    private static void FindLargestRect(Span<int> heights, ref int maxArea, int j,
        ref int maxAreaWidth, ref int maxAreaHeight, ref int maxAreaI0, ref int maxAreaJ0)
    {
        Span<int> stack = stackalloc int[(int)WorldConstants.SegmentLength + 1];
        var stackTop = -1;
        for (var i = 0; i < heights.Length; ++i)
        {
            while (stackTop >= 0 && heights[stack[stackTop]] >= heights[i])
            {
                // heights[i] is next smallest element for top of stack
                // Note that stack is in increasing order, so element below top is previous smallest.
                // Pop rectangle and compare to the current max.
                var currentHeight = heights[stack[stackTop]];
                stackTop--;
                var i0 = stackTop >= 0 ? stack[stackTop] + 1 : 0;
                var width = i - i0;
                var area = width * currentHeight;
                if (area > maxArea)
                {
                    maxArea = area;
                    maxAreaWidth = width;
                    maxAreaHeight = currentHeight;
                    maxAreaI0 = i0;
                    maxAreaJ0 = j - currentHeight + 1;
                }
            }

            // Otherwise if the current position is higher, push it onto the stack.
            if (stackTop < 0 || heights[stack[stackTop]] < heights[i])
            {
                stackTop++;
                stack[stackTop] = i;
            }
        }

        // Anything left on the stack when we get here has no "next smaller" position, so they
        // form rectangles that run up to the right edge of the world segment.
        for (var k = stackTop; k >= 0; --k)
        {
            var i0 = stack[k];

            var width = heights.Length - i0;
            var height = heights[i0];
            var area = width * height;
            if (area > maxArea)
            {
                maxArea = area;
                maxAreaWidth = width;
                maxAreaHeight = height;
                maxAreaI0 = i0;
                maxAreaJ0 = j - height + 1;
            }
        }
    }

    /// <summary>
    ///     Extracts the given rectangle to create a new collision slab.
    /// </summary>
    /// <param name="slabs">List of collision slabs to update.</param>
    /// <param name="basePos">Base position of world segment at the correct z plane.</param>
    /// <param name="i0">Starting column index into block presence grid.</param>
    /// <param name="j0">Starting row index into block presence grid.</param>
    /// <param name="width">Width of rectangle.</param>
    /// <param name="height">Height of rectangle.</param>
    /// <param name="tempGrid">Working copy of block presence grid to update.</param>
    private static void ExtractRect(List<BoundingBox> slabs, Vector3 basePos, int i0, int j0,
        int width, int height, Span<bool> tempGrid)
    {
        slabs.Add(new BoundingBox
        {
            Position = basePos + new Vector3(i0, j0, 0.0f),
            Size = new Vector3(width, height, 1.0f)
        });
        for (var i = 0; i < width; ++i)
        for (var j = 0; j < height; ++j)
        {
            var idx = (int)WorldConstants.SegmentLength * (j0 + j) + i0 + i;
            tempGrid[idx] = false;
        }
    }
}