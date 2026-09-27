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

using System;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Property tests for <see cref="SlopeRelaxation" />: after relaxation the height field
///     steps by at most one block off cliffs, no cell is raised above its pre-relaxation
///     value, and the global minimum is unchanged.
/// </summary>
public class TestSlopeRelaxation
{
    private const int FieldCases = 24;

    [Fact]
    public void Relax_RandomFields_SatisfiesPostConditions()
    {
        var state = 0x5EED_0001UL;

        for (var fieldCase = 0; fieldCase < FieldCases; ++fieldCase)
        {
            var width = 16 + (int)(SeedDerivation.Next(ref state) % 32);
            var height = 16 + (int)(SeedDerivation.Next(ref state) % 32);
            var heights = RandomField(width, height, ref state, 40);
            var original = (int[,])heights.Clone();

            var cliffs = new bool[width, height];
            SlopeRelaxation.Relax(heights, cliffs);

            AssertPostConditions(original, heights, cliffs, width, height, fieldCase);
        }
    }

    [Fact]
    public void Relax_WithMask_FixedCellsUnchanged_AndOffMaskConstrained()
    {
        var state = 0x5EED_0002UL;
        var width = 24;
        var height = 24;
        var heights = RandomField(width, height, ref state, 30);
        var original = (int[,])heights.Clone();

        var movable = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                movable[x, y] = (x / 8 + y / 8) % 2 == 0;
            }
        }

        var cliffs = new bool[width, height];
        SlopeRelaxation.Relax(heights, cliffs, movable);

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (!movable[x, y])
                {
                    Assert.Equal(original[x, y], heights[x, y]);
                    continue;
                }

                Assert.True(heights[x, y] <= original[x, y],
                    $"Cell ({x},{y}) was raised from {original[x, y]} to {heights[x, y]}.");
            }
        }
    }

    [Fact]
    public void Relax_LowersPeaksNeverRaisesValleys()
    {
        var heights = new int[8, 8];
        for (var y = 0; y < 8; ++y)
        {
            for (var x = 0; x < 8; ++x)
            {
                heights[x, y] = 10;
            }
        }

        heights[4, 4] = 20;
        heights[2, 6] = 0;

        var original = (int[,])heights.Clone();
        var cliffs = new bool[8, 8];
        SlopeRelaxation.Relax(heights, cliffs);

        for (var y = 0; y < 8; ++y)
        {
            for (var x = 0; x < 8; ++x)
            {
                Assert.True(heights[x, y] <= original[x, y],
                    $"Cell ({x},{y}) was raised from {original[x, y]} to {heights[x, y]}.");
            }
        }

        Assert.Equal(0, heights[2, 6]);
        Assert.True(heights[4, 4] < 20, "The isolated peak should have been lowered.");
    }

    /// <summary>
    ///     Asserts the relaxation post-conditions over a relaxed field.
    /// </summary>
    /// <param name="original">Pre-relaxation heights.</param>
    /// <param name="heights">Post-relaxation heights.</param>
    /// <param name="cliffs">Cliff flags from relaxation.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="fieldCase">Test case index for failure messages.</param>
    private static void AssertPostConditions(int[,] original, int[,] heights, bool[,] cliffs,
        int width, int height, int fieldCase)
    {
        var globalMinBefore = int.MaxValue;
        var globalMinAfter = int.MaxValue;

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (original[x, y] < globalMinBefore) globalMinBefore = original[x, y];
                if (heights[x, y] < globalMinAfter) globalMinAfter = heights[x, y];

                Assert.True(heights[x, y] <= original[x, y],
                    $"Case {fieldCase}: cell ({x},{y}) was raised from {original[x, y]} to {heights[x, y]}.");

                var maxStep = cliffs[x, y] ? 2 : 1;
                if (x < width - 1)
                {
                    var step = Math.Abs(heights[x, y] - heights[x + 1, y]);
                    var pairMax = Math.Max(maxStep, cliffs[x + 1, y] ? 2 : 1);
                    Assert.True(step <= pairMax,
                        $"Case {fieldCase}: step {step} between ({x},{y}) and ({x + 1},{y}).");
                }

                if (y < height - 1)
                {
                    var step = Math.Abs(heights[x, y] - heights[x, y + 1]);
                    var pairMax = Math.Max(maxStep, cliffs[x, y + 1] ? 2 : 1);
                    Assert.True(step <= pairMax,
                        $"Case {fieldCase}: step {step} between ({x},{y}) and ({x},{y + 1}).");
                }
            }
        }

        Assert.Equal(globalMinBefore, globalMinAfter);
    }

    /// <summary>
    ///     Fills a field with deterministic pseudo-random heights.
    /// </summary>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <param name="state">Random stream state.</param>
    /// <param name="range">Exclusive upper bound of values.</param>
    /// <returns>Random height field.</returns>
    private static int[,] RandomField(int width, int height, ref ulong state, int range)
    {
        var heights = new int[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                heights[x, y] = (int)(SeedDerivation.Next(ref state) % (ulong)range);
            }
        }

        return heights;
    }
}
