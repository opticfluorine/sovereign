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
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Regression tests for coast complexity: the land/ocean boundary of a generated plan
///     must stay irregular. A boundary trace with too few direction changes indicates an
///     over-smoothed coast; too many indicates swirl artifacts imprinted by domain warping.
/// </summary>
public class TestCoastComplexity
{
    /// <summary>
    ///     Fixed seed used by the coastline tests.
    /// </summary>
    private const ulong Seed = 12345;

    /// <summary>
    ///     Accepted band for coastline direction changes along the traced boundary of the
    ///     largest land mass of the 128x128 baseline plan. Calibrated against the tuned
    ///     constants (warp amplitudes 0.35/0.40 with three warp octaves): the traced coast
    ///     of seed 12345 measured 327 direction changes, while the same plan with the warp
    ///     disabled (over-smoothed coast) measured 255 and with the pre-tuning swirl
    ///     amplitudes 0.75/0.85 measured 575. The band catches both regressions.
    /// </summary>
    private const int MinDirectionChanges = 280;

    private const int MaxDirectionChanges = 470;

    [Fact]
    public void Plan_CoastlineComplexity_StaysWithinCalibratedBand()
    {
        var profile = TestProfiles.CreateSmall128();
        var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0,
            System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                $"worldgen-test-coast-{Guid.NewGuid():N}.png"), null);

        var changes = CountBoundaryDirectionChanges(plan.Terrain.IsOcean, profile.Width,
            profile.Height);

        Assert.InRange(changes, MinDirectionChanges, MaxDirectionChanges);
    }

    /// <summary>
    ///     Traces the coastline of the largest land mass from the topmost-leftmost of its
    ///     cells that has water or the field border to the north, walking a Moore-neighbor
    ///     boundary loop, and counts the direction changes between consecutive trace steps.
    /// </summary>
    /// <param name="isOcean">Ocean flags indexed [x, y].</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>Number of direction changes along the traced boundary.</returns>
    private static int CountBoundaryDirectionChanges(bool[,] isOcean, int width, int height)
    {
        // Clockwise 8-neighborhood starting at east.
        var dx = new[] { 1, 1, 0, -1, -1, -1, 0, 1 };
        var dy = new[] { 0, 1, 1, 1, 0, -1, -1, -1 };

        var start = FindLargestComponentStart(isOcean, width, height);
        Assert.True(start.x >= 0, "The plan should contain a coastline.");

        var directions = new List<int>();
        var (cx, cy) = start;
        var backtrack = 6;
        const int maxSteps = 4 * 16384;

        for (var step = 0; step < maxSteps; ++step)
        {
            var found = -1;
            for (var i = 1; i <= 8; ++i)
            {
                var d = (backtrack + i) % 8;
                var nx = cx + dx[d];
                var ny = cy + dy[d];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (isOcean[nx, ny]) continue;

                found = d;
                break;
            }

            Assert.True(found >= 0, "The boundary trace left the land mass.");

            if (directions.Count > 0 && cx == start.x && cy == start.y && found == directions[0])
            {
                break;
            }

            directions.Add(found);
            cx += dx[found];
            cy += dy[found];
            backtrack = (found + 5) % 8;
        }

        Assert.True(directions.Count > 0, "The boundary trace recorded no steps.");

        var changes = 0;
        for (var i = 1; i < directions.Count; ++i)
        {
            if (directions[i] != directions[i - 1]) ++changes;
        }

        return changes;
    }

    /// <summary>
    ///     Finds the trace start on the largest 4-connected land component: its topmost
    ///     leftmost cell with water or the field border to the north.
    /// </summary>
    /// <param name="isOcean">Ocean flags indexed [x, y].</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>The start coordinates, or (-1, -1) if the field is all ocean.</returns>
    private static (int x, int y) FindLargestComponentStart(bool[,] isOcean, int width, int height)
    {
        var component = new int[width, height];
        var sizes = new List<int> { 0 };
        var queue = new Queue<(int x, int y)>();

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (isOcean[x, y] || component[x, y] != 0) continue;

                var id = sizes.Count;
                var size = 0;
                component[x, y] = id;
                queue.Enqueue((x, y));
                while (queue.Count > 0)
                {
                    var (cx, cy) = queue.Dequeue();
                    ++size;
                    if (cx > 0 && !isOcean[cx - 1, cy] && component[cx - 1, cy] == 0)
                    {
                        component[cx - 1, cy] = id;
                        queue.Enqueue((cx - 1, cy));
                    }

                    if (cx < width - 1 && !isOcean[cx + 1, cy] && component[cx + 1, cy] == 0)
                    {
                        component[cx + 1, cy] = id;
                        queue.Enqueue((cx + 1, cy));
                    }

                    if (cy > 0 && !isOcean[cx, cy - 1] && component[cx, cy - 1] == 0)
                    {
                        component[cx, cy - 1] = id;
                        queue.Enqueue((cx, cy - 1));
                    }

                    if (cy < height - 1 && !isOcean[cx, cy + 1] && component[cx, cy + 1] == 0)
                    {
                        component[cx, cy + 1] = id;
                        queue.Enqueue((cx, cy + 1));
                    }
                }

                sizes.Add(size);
            }
        }

        var largest = 0;
        for (var i = 1; i < sizes.Count; ++i)
        {
            if (sizes[i] > sizes[largest]) largest = i;
        }

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (component[x, y] != largest) continue;
                if (y == 0 || isOcean[x, y - 1]) return (x, y);
            }
        }

        return (-1, -1);
    }
}
