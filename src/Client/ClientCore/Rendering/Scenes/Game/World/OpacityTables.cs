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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Lookup tables of overdraw opacity factors.
/// </summary>
public sealed class OpacityTables
{
    /// <summary>
    ///     Number of draw counts to compute lookup tables for.
    /// </summary>
    private const int MaxDraws = 8;

    /// <summary>
    ///     Number of alpha bins for each table.
    /// </summary>
    private const int BinCount = 256;

    private const float BinWidth = 1.0f / BinCount;

    private readonly float[,] tables = new float[MaxDraws, BinCount];

    public OpacityTables()
    {
        for (var n = 0; n < MaxDraws; ++n)
        for (var i = 0; i < BinCount; ++i)
            tables[n, i] = ComputeFactor(n + 2, i);
    }

    /// <summary>
    ///     Gets the multi-draw alpha value which gives the same result as blending without overdraw
    ///     with the given alpha value.
    /// </summary>
    /// <param name="drawCount">Total number of draws.</param>
    /// <param name="alpha">Alpha value that would be used without overdraw.</param>
    /// <returns>Alpha value to use with overdraw.</returns>
    public float GetAlpha(int drawCount, float alpha)
    {
        if (drawCount <= 1) return alpha;
        var bin = Math.Clamp((int)(alpha * BinCount), 0, BinCount - 1);
        return drawCount > MaxDraws + 1 ? ComputeFactor(drawCount, bin) : tables[drawCount - 2, bin];
    }

    /// <summary>
    ///     Computes the overdraw alpha factor for the given bin.
    /// </summary>
    /// <param name="drawCount">Total number of draws.</param>
    /// <param name="bin">Bin index.</param>
    /// <returns>Alpha value to use with overdraw.</returns>
    private float ComputeFactor(int drawCount, int bin)
    {
        var alphaBin = bin * BinWidth;
        return 1.0f - (float)Math.Pow(1.0f - alphaBin, 1.0f / drawCount);
    }
}