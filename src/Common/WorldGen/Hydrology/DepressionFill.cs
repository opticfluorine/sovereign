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

namespace Sovereign.WorldGen.Hydrology;

/// <summary>
///     Priority-flood fill (Barnes) of depressions in a height field so that every cell drains
///     to the border. Border heights are left unchanged, and fill only raises cells.
///     With epsilon enabled, every cell ends with a strictly descending drainage path to the
///     border; with epsilon disabled, depressions are raised exactly to their sill level, which
///     is the water plane of a lake.
/// </summary>
public static class DepressionFill
{
    /// <summary>
    ///     Fills depressions in the given height field in place so that every cell drains to the
    ///     border, with epsilon increments along the fill path for flat spills. The epsilon
    ///     lives only in the routing surface, never in the terrain map.
    /// </summary>
    /// <param name="heights">Height field indexed [x, y], updated in place.</param>
    public static void Fill(int[,] heights)
    {
        Fill(heights, useEpsilon: true);
    }

    /// <summary>
    ///     Fills depressions in the given height field in place, raising depressions exactly to
    ///     their sill level.
    /// </summary>
    /// <param name="heights">Height field indexed [x, y], updated in place.</param>
    /// <param name="useEpsilon">Whether to add epsilon increments along the fill path.</param>
    public static void Fill(int[,] heights, bool useEpsilon)
    {
        var width = heights.GetLength(0);
        var height = heights.GetLength(1);
        var visited = new bool[width, height];
        var heap = new PriorityQueue<int, long>();

        var counter = 0;
        for (var x = 0; x < width; ++x)
        {
            counter = Push(heap, visited, heights, x, 0, counter);
            counter = Push(heap, visited, heights, x, height - 1, counter);
        }

        for (var y = 1; y < height - 1; ++y)
        {
            counter = Push(heap, visited, heights, 0, y, counter);
            counter = Push(heap, visited, heights, width - 1, y, counter);
        }

        while (heap.TryDequeue(out var flatIndex, out _))
        {
            var x = flatIndex % width;
            var y = flatIndex / width;
            var filled = heights[x, y];

            if (x > 0) counter = Flood(heap, visited, heights, x - 1, y, filled, useEpsilon, counter);
            if (x < width - 1) counter = Flood(heap, visited, heights, x + 1, y, filled, useEpsilon, counter);
            if (y > 0) counter = Flood(heap, visited, heights, x, y - 1, filled, useEpsilon, counter);
            if (y < height - 1) counter = Flood(heap, visited, heights, x, y + 1, filled, useEpsilon, counter);
        }
    }

    /// <summary>
    ///     Floods one neighbor of a popped cell, raising it to the spill level if needed.
    /// </summary>
    /// <param name="heap">Priority queue of discovered cells.</param>
    /// <param name="visited">Visited flags.</param>
    /// <param name="heights">Height field.</param>
    /// <param name="x">Neighbor X coordinate.</param>
    /// <param name="y">Neighbor Y coordinate.</param>
    /// <param name="filled">Filled height of the popped cell.</param>
    /// <param name="useEpsilon">Whether to add an epsilon increment over the popped cell.</param>
    /// <param name="counter">Insertion counter for deterministic heap ordering.</param>
    /// <returns>Updated insertion counter.</returns>
    private static int Flood(PriorityQueue<int, long> heap, bool[,] visited, int[,] heights,
        int x, int y, int filled, bool useEpsilon, int counter)
    {
        if (visited[x, y]) return counter;

        var spill = useEpsilon ? Math.Max(heights[x, y], filled + 1) : Math.Max(heights[x, y], filled);
        heights[x, y] = spill;
        return Push(heap, visited, heights, x, y, counter);
    }

    /// <summary>
    ///     Pushes a cell onto the heap with a deterministic priority.
    /// </summary>
    /// <param name="heap">Priority queue of discovered cells.</param>
    /// <param name="visited">Visited flags.</param>
    /// <param name="heights">Height field.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="counter">Insertion counter for deterministic heap ordering.</param>
    /// <returns>Updated insertion counter.</returns>
    private static int Push(PriorityQueue<int, long> heap, bool[,] visited, int[,] heights,
        int x, int y, int counter)
    {
        if (visited[x, y]) return counter;

        visited[x, y] = true;
        var width = heights.GetLength(0);
        heap.Enqueue(y * width + x, ((long)heights[x, y] << 32) | (uint)counter);
        return counter + 1;
    }
}
