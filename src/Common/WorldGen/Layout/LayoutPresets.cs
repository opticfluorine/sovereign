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
using System.Linq;
using Sovereign.WorldGen.Noise;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Generates named anchor layouts parametrically from a seed. Preset anchor centers are
///     kept within [0.15, 0.85] so the border falloff is not fought; scattered presets use
///     seeded Poisson-disc spacing with a minimum separation of twice the anchor radius.
/// </summary>
public static class LayoutPresets
{
    /// <summary>
    ///     Presets recognized by name.
    /// </summary>
    public static readonly IReadOnlyList<string> Names = new[]
    {
        "archipelago", "continents3", "pangaea", "eastwest", "random"
    };

    /// <summary>
    ///     Minimum preset anchor center coordinate.
    /// </summary>
    private const float PlacementMin = 0.15f;

    /// <summary>
    ///     Maximum preset anchor center coordinate.
    /// </summary>
    private const float PlacementMax = 0.85f;

    /// <summary>
    ///     Determines whether a name is a known preset.
    /// </summary>
    /// <param name="name">Preset name.</param>
    /// <returns>true if the preset is known, false otherwise.</returns>
    public static bool IsKnown(string? name)
    {
        return name is not null && Names.Contains(name, StringComparer.Ordinal);
    }

    /// <summary>
    ///     Resolves a preset to its anchor list.
    /// </summary>
    /// <param name="preset">Preset name.</param>
    /// <param name="anchorCount">Anchor count for the random preset.</param>
    /// <param name="seed">Layout sub-seed.</param>
    /// <returns>Resolved anchors, ordered left to right.</returns>
    /// <exception cref="ArgumentException">If the preset name is unknown.</exception>
    public static List<LayoutAnchor> Resolve(string preset, int anchorCount, ulong seed)
    {
        var state = SeedDerivation.DeriveSubSeed(seed, $"Layout.Preset.{preset}");
        return preset switch
        {
            "archipelago" => Archipelago(ref state),
            "continents3" => Continents3(),
            "pangaea" => Pangaea(),
            "eastwest" => EastWest(),
            "random" => Random(anchorCount, ref state),
            _ => throw new ArgumentException($"Unknown layout preset \"{preset}\".", nameof(preset))
        };
    }

    /// <summary>
    ///     Builds the continents3 preset: three anchors left to right with the middle one
    ///     smaller and carrying a mountain bias.
    /// </summary>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> Continents3()
    {
        return new List<LayoutAnchor>
        {
            new() { X = 0.18f, Y = 0.50f, Radius = 0.16f, Weight = 1.0f },
            new()
            {
                X = 0.50f, Y = 0.50f, Radius = 0.11f, Weight = 0.7f, MountainBias = 1.0f
            },
            new() { X = 0.82f, Y = 0.50f, Radius = 0.16f, Weight = 0.9f }
        };
    }

    /// <summary>
    ///     Builds the pangaea preset: one large central anchor.
    /// </summary>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> Pangaea()
    {
        return new List<LayoutAnchor>
        {
            new() { X = 0.50f, Y = 0.50f, Radius = 0.30f, Weight = 1.0f }
        };
    }

    /// <summary>
    ///     Builds the eastwest preset: two anchors straddling the equator.
    /// </summary>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> EastWest()
    {
        return new List<LayoutAnchor>
        {
            new() { X = 0.28f, Y = 0.50f, Radius = 0.17f, Weight = 1.0f },
            new() { X = 0.72f, Y = 0.50f, Radius = 0.17f, Weight = 1.0f }
        };
    }

    /// <summary>
    ///     Builds the archipelago preset: 8 to 12 small scattered anchors.
    /// </summary>
    /// <param name="state">Random stream state.</param>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> Archipelago(ref ulong state)
    {
        var count = 8 + (int)(SeedDerivation.Next(ref state) % 5);
        return ScatterPoisson(count, 0.06f, 0.10f, 0.5f, 0.8f, ref state);
    }

    /// <summary>
    ///     Builds the random preset: a configured count of scattered anchors.
    /// </summary>
    /// <param name="anchorCount">Number of anchors.</param>
    /// <param name="state">Random stream state.</param>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> Random(int anchorCount, ref ulong state)
    {
        var count = Math.Clamp(anchorCount, 1, 16);
        return Scatter(count, 0.10f, 0.16f, 1.0f, 1.0f, ref state);
    }

    /// <summary>
    ///     Scatters anchors with Bridson's Poisson-disc sampling, guaranteeing that every
    ///     pair of anchors is at least twice the maximum anchor radius apart. Returns at most
    ///     the requested count; the placement box may hold fewer.
    /// </summary>
    /// <param name="count">Maximum number of anchors to place.</param>
    /// <param name="minRadius">Minimum anchor radius.</param>
    /// <param name="maxRadius">Maximum anchor radius.</param>
    /// <param name="minWeight">Minimum anchor weight.</param>
    /// <param name="maxWeight">Maximum anchor weight.</param>
    /// <param name="state">Random stream state.</param>
    /// <returns>Anchors with guaranteed minimum separation.</returns>
    private static List<LayoutAnchor> ScatterPoisson(int count, float minRadius, float maxRadius,
        float minWeight, float maxWeight, ref ulong state)
    {
        const int attemptsPerActive = 30;
        var minSeparation = 2f * maxRadius;
        var cellSize = minSeparation / MathF.Sqrt(2f);
        var gridSize = (int)MathF.Ceiling((PlacementMax - PlacementMin) / cellSize) + 1;
        var grid = new LayoutAnchor?[gridSize, gridSize];
        var active = new List<LayoutAnchor>();
        var result = new List<LayoutAnchor>();

        var first = MakeCandidate(minRadius, maxRadius, minWeight, maxWeight, ref state);
        AddToPoisson(first, result, active, grid, cellSize);

        while (active.Count > 0 && result.Count < count)
        {
            var index = (int)(SeedDerivation.Next(ref state) % (ulong)active.Count);
            var origin = active[index];
            var placed = false;
            for (var k = 0; k < attemptsPerActive; ++k)
            {
                var angle = 2f * MathF.PI * NextFloat(ref state);
                var distance = minSeparation * (1f + NextFloat(ref state));
                var template = MakeCandidate(minRadius, maxRadius, minWeight, maxWeight, ref state);
                var candidate = new LayoutAnchor
                {
                    X = origin.X + distance * MathF.Cos(angle),
                    Y = origin.Y + distance * MathF.Sin(angle),
                    Radius = template.Radius,
                    Weight = template.Weight
                };

                if (candidate.X < PlacementMin || candidate.X > PlacementMax
                    || candidate.Y < PlacementMin || candidate.Y > PlacementMax)
                {
                    continue;
                }

                if (HasPoissonNeighbor(grid, cellSize, minSeparation, candidate)) continue;

                AddToPoisson(candidate, result, active, grid, cellSize);
                placed = true;
                break;
            }

            if (!placed) active.RemoveAt(index);
        }

        result.Sort((a, b) => a.X.CompareTo(b.X));
        return result;
    }

    /// <summary>
    ///     Adds a point to the Poisson result, active list, and background grid.
    /// </summary>
    /// <param name="anchor">Anchor to add.</param>
    /// <param name="result">Accepted anchors.</param>
    /// <param name="active">Active anchors.</param>
    /// <param name="grid">Background grid.</param>
    /// <param name="cellSize">Grid cell size.</param>
    private static void AddToPoisson(LayoutAnchor anchor, List<LayoutAnchor> result,
        List<LayoutAnchor> active, LayoutAnchor?[,] grid, float cellSize)
    {
        result.Add(anchor);
        active.Add(anchor);
        var (gx, gy) = GridCell(anchor, cellSize, grid);
        grid[gx, gy] = anchor;
    }

    /// <summary>
    ///     Determines whether a candidate lies within the minimum separation of any accepted
    ///     anchor in the surrounding grid cells.
    /// </summary>
    /// <param name="grid">Background grid.</param>
    /// <param name="cellSize">Grid cell size.</param>
    /// <param name="minSeparation">Minimum permitted separation.</param>
    /// <param name="candidate">Candidate anchor.</param>
    /// <returns>true if a neighbor is too close, false otherwise.</returns>
    private static bool HasPoissonNeighbor(LayoutAnchor?[,] grid, float cellSize,
        float minSeparation, LayoutAnchor candidate)
    {
        var (gx, gy) = GridCell(candidate, cellSize, grid);
        for (var oy = gy - 2; oy <= gy + 2; ++oy)
        {
            if (oy < 0 || oy >= grid.GetLength(1)) continue;
            for (var ox = gx - 2; ox <= gx + 2; ++ox)
            {
                if (ox < 0 || ox >= grid.GetLength(0)) continue;
                var other = grid[ox, oy];
                if (other is null) continue;
                var dx = other.X - candidate.X;
                var dy = other.Y - candidate.Y;
                if (dx * dx + dy * dy < minSeparation * minSeparation) return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Computes the background grid cell of an anchor, clamped to the grid bounds.
    /// </summary>
    /// <param name="anchor">Anchor.</param>
    /// <param name="cellSize">Grid cell size.</param>
    /// <param name="grid">Background grid.</param>
    /// <returns>Grid cell coordinates.</returns>
    private static (int X, int Y) GridCell(LayoutAnchor anchor, float cellSize, LayoutAnchor?[,] grid)
    {
        var gx = Math.Clamp((int)((anchor.X - PlacementMin) / cellSize), 0, grid.GetLength(0) - 1);
        var gy = Math.Clamp((int)((anchor.Y - PlacementMin) / cellSize), 0, grid.GetLength(1) - 1);
        return (gx, gy);
    }

    /// <summary>
    ///     Scatters anchors with seeded rejection sampling subject to the twice-radius
    ///     minimum separation; separation is relaxed only if the requested count cannot
    ///     otherwise be placed in the placement box.
    /// </summary>
    /// <param name="count">Number of anchors to place.</param>
    /// <param name="minRadius">Minimum anchor radius.</param>
    /// <param name="maxRadius">Maximum anchor radius.</param>
    /// <param name="minWeight">Minimum anchor weight.</param>
    /// <param name="maxWeight">Maximum anchor weight.</param>
    /// <param name="state">Random stream state.</param>
    /// <returns>Anchors.</returns>
    private static List<LayoutAnchor> Scatter(int count, float minRadius, float maxRadius,
        float minWeight, float maxWeight, ref ulong state)
    {
        var anchors = new List<LayoutAnchor>(count);
        var separationScale = 1f;
        var maxAttempts = count * 2000;
        var attempts = 0;
        while (anchors.Count < count && attempts < maxAttempts)
        {
            ++attempts;
            var candidate = MakeCandidate(minRadius, maxRadius, minWeight, maxWeight, ref state);
            if (anchors.All(a => Separation(a, candidate) >= separationScale)) anchors.Add(candidate);

            if (attempts % (count * 200) == 0) separationScale = MathF.Max(0.5f * separationScale, 0.25f);
        }

        // Best-effort fill if the box was too crowded for the requested separation.
        while (anchors.Count < count)
        {
            anchors.Add(MakeCandidate(minRadius, maxRadius, minWeight, maxWeight, ref state));
        }

        anchors.Sort((a, b) => a.X.CompareTo(b.X));
        return anchors;
    }

    /// <summary>
    ///     Creates one random candidate anchor.
    /// </summary>
    /// <param name="minRadius">Minimum anchor radius.</param>
    /// <param name="maxRadius">Maximum anchor radius.</param>
    /// <param name="minWeight">Minimum anchor weight.</param>
    /// <param name="maxWeight">Maximum anchor weight.</param>
    /// <param name="state">Random stream state.</param>
    /// <returns>Candidate anchor.</returns>
    private static LayoutAnchor MakeCandidate(float minRadius, float maxRadius, float minWeight,
        float maxWeight, ref ulong state)
    {
        var radius = Lerp(minRadius, maxRadius, NextFloat(ref state));
        return new LayoutAnchor
        {
            X = Lerp(PlacementMin, PlacementMax, NextFloat(ref state)),
            Y = Lerp(PlacementMin, PlacementMax, NextFloat(ref state)),
            Radius = radius,
            Weight = Lerp(minWeight, maxWeight, NextFloat(ref state))
        };
    }

    /// <summary>
    ///     Computes the twice-radius minimum separation required between two anchors.
    /// </summary>
    /// <param name="a">First anchor.</param>
    /// <param name="b">Second anchor.</param>
    /// <returns>Minimum permitted normalized Euclidean separation.</returns>
    private static float Separation(LayoutAnchor a, LayoutAnchor b)
    {
        return 2f * MathF.Max(a.Radius, b.Radius);
    }

    /// <summary>
    ///     Samples a uniform float in [0, 1) from the random stream.
    /// </summary>
    /// <param name="state">Random stream state.</param>
    /// <returns>Sample in [0, 1).</returns>
    private static float NextFloat(ref ulong state)
    {
        const float scale = 1f / (1UL << 24);
        return (SeedDerivation.Next(ref state) >> 40) * scale;
    }

    /// <summary>
    ///     Linearly interpolates between two values.
    /// </summary>
    /// <param name="from">Value at t = 0.</param>
    /// <param name="to">Value at t = 1.</param>
    /// <param name="t">Interpolation factor.</param>
    /// <returns>Interpolated value.</returns>
    private static float Lerp(float from, float to, float t)
    {
        return from + (to - from) * t;
    }
}
