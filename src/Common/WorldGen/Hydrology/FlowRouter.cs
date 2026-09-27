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

namespace Sovereign.WorldGen.Hydrology;

/// <summary>
///     Routed flow surface for a plan.
/// </summary>
public sealed class FlowRouting
{
    /// <summary>
    ///     Flat index of each cell's D8 receiver, or -1 for cells that drain off the border.
    /// </summary>
    public required int[,] Receiver { get; init; }

    /// <summary>
    ///     Draining area of each cell in cells, including the cell itself.
    /// </summary>
    public required int[,] Accumulation { get; init; }
}

/// <summary>
///     Routes flow over a filled height field with D8 steepest descent receivers and
///     computes flow accumulation in a single pass over cells sorted by descending
///     filled height. Ties in descent steepness are broken by a seeded hash.
/// </summary>
public sealed class FlowRouter
{
    /// <summary>
    ///     Relative weight of diagonal steps; the reciprocal of the diagonal step length.
    /// </summary>
    private const float DiagonalWeight = 0.70710678118654752f;

    private readonly ulong seed;

    /// <summary>
    ///     Creates a flow router.
    /// </summary>
    /// <param name="seed">Sub-seed used to break descent ties.</param>
    public FlowRouter(ulong seed)
    {
        this.seed = seed;
    }

    /// <summary>
    ///     Routes flow over the given filled height field.
    /// </summary>
    /// <param name="filledHeights">Filled height field indexed [x, y].</param>
    /// <returns>Routing receivers and flow accumulation.</returns>
    public FlowRouting Route(int[,] filledHeights)
    {
        var width = filledHeights.GetLength(0);
        var height = filledHeights.GetLength(1);
        var receiver = new int[width, height];
        var accumulation = new int[width, height];

        ParallelComputeReceivers(filledHeights, receiver, width, height);
        Accumulate(filledHeights, receiver, accumulation, width, height);

        return new FlowRouting { Receiver = receiver, Accumulation = accumulation };
    }

    /// <summary>
    ///     Computes the D8 receiver of every cell in parallel over scanlines.
    /// </summary>
    /// <param name="filledHeights">Filled height field.</param>
    /// <param name="receiver">Receiver flat indices, updated in place.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    private void ParallelComputeReceivers(int[,] filledHeights, int[,] receiver, int width, int height)
    {
        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                receiver[x, y] = ReceiverOf(filledHeights, x, y, width, height);
            }
        });
    }

    /// <summary>
    ///     Finds the steepest descent receiver of a cell, breaking ties by seeded hash.
    /// </summary>
    /// <param name="filledHeights">Filled height field.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    /// <returns>Flat index of the receiver, or -1 if the cell drains off the border.</returns>
    private int ReceiverOf(int[,] filledHeights, int x, int y, int width, int height)
    {
        var h = filledHeights[x, y];
        var bestSlope = 0.0;
        var bestHash = 0UL;
        var best = -1;

        for (var direction = 0; direction < 8; ++direction)
        {
            var nx = x + DirectionDx[direction];
            var ny = y + DirectionDy[direction];
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

            var drop = (double)(h - filledHeights[nx, ny]);
            if (drop <= 0.0) continue;

            var slope = drop * (direction < 4 ? 1.0 : DiagonalWeight);
            if (slope < bestSlope) continue;

            var hash = HashOf(nx, ny);
            if (slope > bestSlope || hash > bestHash)
            {
                bestSlope = slope;
                bestHash = hash;
                best = ny * width + nx;
            }
        }

        return best;
    }

    /// <summary>
    ///     Accumulates flow in one pass over cells sorted by descending filled height.
    /// </summary>
    /// <param name="filledHeights">Filled height field.</param>
    /// <param name="receiver">Receiver flat indices.</param>
    /// <param name="accumulation">Accumulation, updated in place.</param>
    /// <param name="width">Field width.</param>
    /// <param name="height">Field height.</param>
    private static void Accumulate(int[,] filledHeights, int[,] receiver, int[,] accumulation,
        int width, int height)
    {
        var count = width * height;
        var order = new int[count];
        var keys = new long[count];
        for (var i = 0; i < count; ++i)
        {
            order[i] = i;
            accumulation[i % width, i / width] = 1;
            keys[i] = ((long)int.MaxValue - filledHeights[i % width, i / width] << 32) | (uint)i;
        }

        Array.Sort(keys, order);

        foreach (var flat in order)
        {
            var x = flat % width;
            var y = flat / width;
            var r = receiver[x, y];
            if (r < 0) continue;

            accumulation[r % width, r / width] += accumulation[x, y];
        }
    }

    /// <summary>
    ///     Computes the seeded tie-break hash of a cell.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Hash value.</returns>
    private ulong HashOf(int x, int y)
    {
        return SeedDerivation.SplitMix64(seed ^ ((ulong)(uint)x << 32 | (uint)y));
    }

    /// <summary>
    ///     X offsets of the eight D8 directions; directions 0..3 are cardinal, 4..7 diagonal.
    /// </summary>
    internal static readonly int[] DirectionDx = { -1, 1, 0, 0, -1, 1, -1, 1 };

    /// <summary>
    ///     Y offsets of the eight D8 directions; directions 0..3 are cardinal, 4..7 diagonal.
    /// </summary>
    internal static readonly int[] DirectionDy = { 0, 0, -1, 1, -1, -1, 1, 1 };
}
