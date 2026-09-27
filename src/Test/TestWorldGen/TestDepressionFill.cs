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
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Noise;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Property tests for <see cref="DepressionFill" />: fill only raises cells, leaves
///     border heights unchanged, and leaves no local minima except along the border.
/// </summary>
public class TestDepressionFill
{
    private const int FieldCases = 16;

    [Fact]
    public void Fill_RandomFields_SatisfiesPostConditions()
    {
        var state = 0xF11D_0001UL;

        for (var fieldCase = 0; fieldCase < FieldCases; ++fieldCase)
        {
            var width = 16 + (int)(SeedDerivation.Next(ref state) % 32);
            var height = 16 + (int)(SeedDerivation.Next(ref state) % 32);
            var heights = RandomField(width, height, ref state);
            var original = (int[,])heights.Clone();

            DepressionFill.Fill(heights);

            AssertNoRaising(original, heights, width, height, fieldCase);
            AssertBorderUnchanged(original, heights, width, height, fieldCase);
            AssertNoInteriorLocalMinima(heights, width, height, fieldCase);
        }
    }

    [Fact]
    public void Fill_SillLevel_RemovesDepressionsWithoutEpsilon()
    {
        var heights = new int[8, 8];
        for (var y = 0; y < 8; ++y)
        {
            for (var x = 0; x < 8; ++x)
            {
                heights[x, y] = 10;
            }
        }

        heights[3, 3] = 4;
        heights[4, 3] = 5;
        heights[3, 4] = 6;

        DepressionFill.Fill(heights, useEpsilon: false);

        // The depression drains through cells of height 10, so the filled level is the sill
        // height 10 and no cell may rise above it.
        for (var y = 0; y < 8; ++y)
        {
            for (var x = 0; x < 8; ++x)
            {
                Assert.InRange(heights[x, y], 4, 10);
            }
        }

        Assert.Equal(10, heights[3, 3]);
    }

    [Fact]
    public void Fill_SillLevel_FlatFieldIsUnchanged()
    {
        var heights = new int[16, 16];
        Array.Clear(heights);
        var original = (int[,])heights.Clone();

        DepressionFill.Fill(heights, useEpsilon: false);

        for (var y = 0; y < 16; ++y)
        {
            for (var x = 0; x < 16; ++x)
            {
                Assert.Equal(original[x, y], heights[x, y]);
            }
        }
    }

    /// <summary>
    ///     Asserts that fill never lowered any cell.
    /// </summary>
    private static void AssertNoRaising(int[,] original, int[,] heights, int width, int height,
        int fieldCase)
    {
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                Assert.True(heights[x, y] >= original[x, y],
                    $"Case {fieldCase}: cell ({x},{y}) was lowered from {original[x, y]} to {heights[x, y]}.");
            }
        }
    }

    /// <summary>
    ///     Asserts that border cells kept their original heights.
    /// </summary>
    private static void AssertBorderUnchanged(int[,] original, int[,] heights, int width, int height,
        int fieldCase)
    {
        for (var x = 0; x < width; ++x)
        {
            Assert.Equal(original[x, 0], heights[x, 0]);
            Assert.Equal(original[x, height - 1], heights[x, height - 1]);
        }

        for (var y = 0; y < height; ++y)
        {
            Assert.Equal(original[0, y], heights[0, y]);
            Assert.Equal(original[width - 1, y], heights[width - 1, y]);
        }
    }

    /// <summary>
    ///     Asserts that every interior cell has a strictly lower cardinal neighbor, so no
    ///     local minimum survives away from the border.
    /// </summary>
    private static void AssertNoInteriorLocalMinima(int[,] heights, int width, int height,
        int fieldCase)
    {
        for (var y = 1; y < height - 1; ++y)
        {
            for (var x = 1; x < width - 1; ++x)
            {
                var h = heights[x, y];
                var hasLower = heights[x - 1, y] < h || heights[x + 1, y] < h
                               || heights[x, y - 1] < h || heights[x, y + 1] < h;
                Assert.True(hasLower,
                    $"Case {fieldCase}: local minimum survived at ({x},{y}) with height {h}.");
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
                heights[x, y] = (int)(SeedDerivation.Next(ref state) % 64) - 32;
            }
        }

        return heights;
    }
}
