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

namespace Sovereign.WorldGen.Terrain;

/// <summary>
///     Chamfer relaxation of the height field enforcing a maximum step of one block between
///     neighboring cells, except across cliffs where a step of two is tolerated.
///     Relaxation lowers peaks and never raises valleys: the global minimum is preserved.
/// </summary>
public static class SlopeRelaxation
{
    /// <summary>
    ///     Maximum height step enforced between neighboring cells away from cliffs.
    /// </summary>
    private const int MaxStep = 1;

    /// <summary>
    ///     Maximum height step tolerated across cliff edges.
    /// </summary>
    private const int CliffStep = 2;

    /// <summary>
    ///     Pre-relaxation slope at which a cell is a cliff candidate.
    /// </summary>
    private const int CliffCandidateSlope = 3;

    /// <summary>
    ///     Post-relaxation slope at which a cliff candidate remains a cliff.
    /// </summary>
    private const int CliffPostSlope = 2;

    /// <summary>
    ///     Upper bound on relaxation sweeps; sweeps converge in a few passes in practice.
    /// </summary>
    private const int MaxPasses = 256;

    /// <summary>
    ///     Relaxes the given height field in place and tags cliff cells.
    /// </summary>
    /// <param name="heights">Height field indexed [x, y], updated in place.</param>
    /// <param name="cliffs">Cliff flags indexed [x, y], updated in place.</param>
    /// <param name="movable">Optional mask of cells that may be relaxed; fixed cells keep their
    /// height and do not constrain their neighbors. All cells are relaxed when null.</param>
    public static void Relax(int[,] heights, bool[,] cliffs, bool[,]? movable = null)
    {
        var width = heights.GetLength(0);
        var height = heights.GetLength(1);
        var original = (int[,])heights.Clone();
        System.Array.Clear(cliffs);

        for (var pass = 0; pass < MaxPasses; ++pass)
        {
            var changed = SweepForward(heights, original, movable, width, height);
            changed |= SweepBackward(heights, original, movable, width, height);
            if (!changed) break;
        }

        TagCliffs(heights, original, cliffs, width, height);
    }

    /// <summary>
    ///     Runs one relaxation sweep in raster order, limiting each cell against its west and
    ///     north neighbors. Cliff edges are limited by the cliff step instead of the standard step.
    /// </summary>
    /// <param name="heights">Height field, updated in place.</param>
    /// <param name="original">Pre-relaxation height field.</param>
    /// <param name="movable">Optional mask of cells that may be relaxed.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>true if any cell changed, false otherwise.</returns>
    private static bool SweepForward(int[,] heights, int[,] original, bool[,]? movable,
        int width, int height)
    {
        var changed = false;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (movable is not null && !movable[x, y]) continue;

                var bound = int.MaxValue;
                if (x > 0 && (movable is null || movable[x - 1, y]))
                {
                    var candidate = heights[x - 1, y] + Step(original, x, y, x - 1, y);
                    if (candidate < bound) bound = candidate;
                }

                if (y > 0 && (movable is null || movable[x, y - 1]))
                {
                    var candidate = heights[x, y - 1] + Step(original, x, y, x, y - 1);
                    if (candidate < bound) bound = candidate;
                }

                if (bound == int.MaxValue || heights[x, y] <= bound) continue;

                heights[x, y] = bound;
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    ///     Runs one relaxation sweep in reverse raster order, limiting each cell against its east
    ///     and south neighbors. Cliff edges are limited by the cliff step instead of the standard step.
    /// </summary>
    /// <param name="heights">Height field, updated in place.</param>
    /// <param name="original">Pre-relaxation height field.</param>
    /// <param name="movable">Optional mask of cells that may be relaxed.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>true if any cell changed, false otherwise.</returns>
    private static bool SweepBackward(int[,] heights, int[,] original, bool[,]? movable,
        int width, int height)
    {
        var changed = false;
        for (var y = height - 1; y >= 0; --y)
        {
            for (var x = width - 1; x >= 0; --x)
            {
                if (movable is not null && !movable[x, y]) continue;

                var bound = int.MaxValue;
                if (x < width - 1 && (movable is null || movable[x + 1, y]))
                {
                    var candidate = heights[x + 1, y] + Step(original, x, y, x + 1, y);
                    if (candidate < bound) bound = candidate;
                }

                if (y < height - 1 && (movable is null || movable[x, y + 1]))
                {
                    var candidate = heights[x, y + 1] + Step(original, x, y, x, y + 1);
                    if (candidate < bound) bound = candidate;
                }

                if (bound == int.MaxValue || heights[x, y] <= bound) continue;

                heights[x, y] = bound;
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    ///     Determines the maximum allowed step from a neighbor to a cell based on the
    ///     pre-relaxation slope of the pair.
    /// </summary>
    /// <param name="original">Pre-relaxation height field.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="nx">Neighbor X coordinate.</param>
    /// <param name="ny">Neighbor Y coordinate.</param>
    /// <returns>Maximum allowed step.</returns>
    private static int Step(int[,] original, int x, int y, int nx, int ny)
    {
        return System.Math.Abs(original[x, y] - original[nx, ny]) >= CliffCandidateSlope
            ? CliffStep
            : MaxStep;
    }

    /// <summary>
    ///     Tags cells as cliffs where the pre-relaxation slope reached the cliff candidate
    ///     threshold and the post-relaxation slope still reaches the cliff slope.
    /// </summary>
    /// <param name="heights">Relaxed height field.</param>
    /// <param name="original">Pre-relaxation height field.</param>
    /// <param name="cliffs">Cliff flags, updated in place.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    private static void TagCliffs(int[,] heights, int[,] original, bool[,] cliffs, int width, int height)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (MaxSlope(original, x, y, width, height) < CliffCandidateSlope)
                {
                    cliffs[x, y] = false;
                    continue;
                }

                cliffs[x, y] = MaxSlope(heights, x, y, width, height) >= CliffPostSlope;
            }
        }
    }

    /// <summary>
    ///     Computes the maximum cardinal slope between a cell and its neighbors.
    /// </summary>
    /// <param name="heights">Height field.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>Maximum slope.</returns>
    private static int MaxSlope(int[,] heights, int x, int y, int width, int height)
    {
        var h = heights[x, y];
        var max = 0;
        if (x > 0) max = System.Math.Max(max, System.Math.Abs(h - heights[x - 1, y]));
        if (y > 0) max = System.Math.Max(max, System.Math.Abs(h - heights[x, y - 1]));
        if (x < width - 1) max = System.Math.Max(max, System.Math.Abs(h - heights[x + 1, y]));
        if (y < height - 1) max = System.Math.Max(max, System.Math.Abs(h - heights[x, y + 1]));
        return max;
    }
}
