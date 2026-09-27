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
using System.Collections.Generic;
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Noise;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Property tests for <see cref="FlowRouter" />: every route follows steepest descent on
///     the filled surface and accumulation is monotonically non-decreasing downstream.
/// </summary>
public class TestFlowRouter
{
    private const int FieldCases = 8;

    [Fact]
    public void Route_RandomFilledFields_RoutesFollowSteepestDescent()
    {
        var state = 0xF10D_0001UL;

        for (var fieldCase = 0; fieldCase < FieldCases; ++fieldCase)
        {
            var width = 16 + (int)(SeedDerivation.Next(ref state) % 16);
            var height = 16 + (int)(SeedDerivation.Next(ref state) % 16);
            var heights = RandomField(width, height, ref state);
            DepressionFill.Fill(heights);

            var routing = new FlowRouter((ulong)fieldCase).Route(heights);

            AssertSteepestDescent(heights, routing, width, height, fieldCase);
            AssertAccumulationMonotone(heights, routing, width, height, fieldCase);
        }
    }

    [Fact]
    public void Route_TiltedPlane_FlowsDownhill()
    {
        var heights = new int[16, 16];
        for (var y = 0; y < 16; ++y)
        {
            for (var x = 0; x < 16; ++x)
            {
                heights[x, y] = 100 - x;
            }
        }

        var routing = new FlowRouter(1).Route(heights);

        for (var y = 0; y < 16; ++y)
        {
            for (var x = 0; x < 16; ++x)
            {
                var expected = x == 15 ? -1 : y * 16 + x + 1;
                Assert.Equal(expected, routing.Receiver[x, y]);
            }
        }

        // Each row drains east, so the rightmost cell of every row accumulates the whole row.
        Assert.Equal(1, routing.Accumulation[0, 0]);
        Assert.Equal(16, routing.Accumulation[15, 0]);
    }

    /// <summary>
    ///     Asserts that each interior route descends the steepest available slope, weighting
    ///     diagonal steps like the router does.
    /// </summary>
    private static void AssertSteepestDescent(int[,] heights, FlowRouting routing, int width,
        int height, int fieldCase)
    {
        const double diagonalWeight = 0.70710678118654752;

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var receiver = routing.Receiver[x, y];
                var h = heights[x, y];
                var bestSlope = 0.0;
                for (var d = 0; d < 8; ++d)
                {
                    var nx = x + FlowRouter.DirectionDx[d];
                    var ny = y + FlowRouter.DirectionDy[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    var drop = h - heights[nx, ny];
                    if (drop <= 0) continue;

                    var slope = drop * (d < 4 ? 1.0 : diagonalWeight);
                    bestSlope = Math.Max(bestSlope, slope);
                }

                if (bestSlope <= 0.0)
                {
                    Assert.Equal(-1, receiver);
                    continue;
                }

                Assert.True(receiver >= 0,
                    $"Case {fieldCase}: cell ({x},{y}) with slope {bestSlope} has no receiver.");
                var rx = receiver % width;
                var ry = receiver / width;
                Assert.True(heights[rx, ry] < h,
                    $"Case {fieldCase}: route from ({x},{y}) does not descend.");
                var dropX = h - heights[rx, ry];
                var isDiagonal = Math.Abs(rx - x) == 1 && Math.Abs(ry - y) == 1;
                var receiverSlope = dropX * (isDiagonal ? diagonalWeight : 1.0);
                Assert.Equal(bestSlope, receiverSlope, 5);
            }
        }
    }

    /// <summary>
    ///     Asserts that accumulation never increases upstream and that every cell drains at
    ///     least itself.
    /// </summary>
    private static void AssertAccumulationMonotone(int[,] heights, FlowRouting routing, int width,
        int height, int fieldCase)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                Assert.True(routing.Accumulation[x, y] >= 1,
                    $"Case {fieldCase}: cell ({x},{y}) accumulates less than itself.");

                var receiver = routing.Receiver[x, y];
                if (receiver < 0) continue;

                var rx = receiver % width;
                var ry = receiver / width;
                Assert.True(routing.Accumulation[rx, ry] >= routing.Accumulation[x, y],
                    $"Case {fieldCase}: accumulation decreases from ({x},{y}) downstream.");
                Assert.True(heights[rx, ry] <= heights[x, y],
                    $"Case {fieldCase}: receiver of ({x},{y}) is not lower on the filled surface.");
            }
        }
    }

    /// <summary>
    ///     Fills a field with deterministic pseudo-random heights.
    /// </summary>
    private static int[,] RandomField(int width, int height, ref ulong state)
    {
        var heights = new int[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                heights[x, y] = (int)(SeedDerivation.Next(ref state) % 128);
            }
        }

        return heights;
    }
}
