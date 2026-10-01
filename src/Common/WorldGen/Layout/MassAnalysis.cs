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
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Why a land mass counts as significant.
/// </summary>
public enum MassSignificanceKind
{
    /// <summary>
    ///     The mass is not significant: decoration land.
    /// </summary>
    None,

    /// <summary>
    ///     Substantially all of the mass lies within the intended footprint of a significant
    ///     anchor.
    /// </summary>
    AnchorBacked,

    /// <summary>
    ///     The mass exceeds the size floor for seed-native landmasses.
    /// </summary>
    SizeFloor
}

/// <summary>
///     One analyzed land mass: its component label, size, normalized centroid, and whether
///     it is anchor-backed and significant.
/// </summary>
/// <param name="ComponentId">1-based 4-connected component label.</param>
/// <param name="CentroidX">Centroid X in normalized footprint coordinates.</param>
/// <param name="CentroidY">Centroid Y in normalized footprint coordinates.</param>
/// <param name="Size">Mass size in cells.</param>
/// <param name="AnchorBacked">Whether at least half of the mass's cells lie within the
/// extended footprint of some significant anchor.</param>
/// <param name="Significant">Whether the mass is significant.</param>
public sealed record MassAnalysisResult(int ComponentId, float CentroidX, float CentroidY,
    long Size, bool AnchorBacked, bool Significant)
{
    /// <summary>
    ///     The extent that made the mass significant, when it is.
    /// </summary>
    public MassSignificanceKind SignificanceKind { get; init; }
        = !Significant ? MassSignificanceKind.None
            : AnchorBacked ? MassSignificanceKind.AnchorBacked
            : MassSignificanceKind.SizeFloor;
}

/// <summary>
///     land-mass analysis of one terrain map: the per-cell component labels and the per-mass
///     significances. Labels are 1-based, 0 for ocean.
/// </summary>
/// <param name="Labels">Component labels indexed <c>[x, y]</c>; 0 for ocean.</param>
/// <param name="Masses">Analysis of every land mass, in scan order of first cell.</param>
public sealed record MassAnalysisReport(int[,] Labels, IReadOnlyList<MassAnalysisResult> Masses)
{
    /// <summary>
    ///     The significant masses only.
    /// </summary>
    public IEnumerable<MassAnalysisResult> SignificantMasses
    {
        get
        {
            foreach (var mass in Masses)
            {
                if (mass.Significant) yield return mass;
            }
        }
    }
}

/// <summary>
///     Shared definition of land-mass significance for the layout validator, layout report,
///     and mountain mask. A mass is <see cref="MassAnalysisResult.AnchorBacked" /> when at
///     least half of its cells lie within <c>Radius * 1.25</c> of a significant anchor's
///     center, and is significant when anchor-backed or larger than
///     <c>max(FloorCells, FloorLandFraction of total land)</c>. Land below significance is
///     decoration: excluded from anchor matching and mountain quotas but still rendered.
/// </summary>
public static class MassAnalysis
{
    /// <summary>
    ///     Fraction of a mass's cells that must lie within a significant anchor's extended
    ///     footprint for the mass to be anchor-backed.
    /// </summary>
    public const float AnchorBackedFraction = 0.5f;

    /// <summary>
    ///     Anchor footprint multiplier: a cell is inside a significant anchor's extended
    ///     footprint when it lies within <c>Radius * 1.25</c> of the anchor center. The 1.25
    ///     slack covers coastline erosion and bias-driven size variance around the intended
    ///     footprint.
    /// </summary>
    public const float AnchorFootprintFactor = 1.25f;

    /// <summary>
    ///     Absolute size floor in cells for seed-native landmasses in weak or absent layouts.
    ///     A mass is significant when strictly larger than the floor.
    /// </summary>
    public const long FloorCells = 400;

    /// <summary>
    ///     Fraction of total land at which a non-anchor-backed mass remains significant.
    ///     Replaces the retired relative-dominance 3% rule, which failed archipelago layouts
    ///     whose equal islands are all significant by intent.
    /// </summary>
    public const float FloorLandFraction = 0.01f;

    /// <summary>
    ///     Minimum anchor weight that participates in anchor matching.
    /// </summary>
    public const float SignificantWeight = 0.5f;

    /// <summary>
    ///     Labels the 4-connected land masses of a terrain map and analyzes their
    ///     significance.
    /// </summary>
    /// <param name="map">Terrain map; only the water flags are read.</param>
    /// <param name="anchors">Resolved layout anchors, or null. Anchors below
    /// <see cref="SignificantWeight" /> never back a mass.</param>
    /// <returns>Labels and per-mass analysis, in scan order of first cell.</returns>
    public static MassAnalysisReport Analyze(TerrainMap map,
        IReadOnlyList<ResolvedAnchor>? anchors)
    {
        var labels = LabelComponents(map, out var components);
        var masses = AnalyzeLabels(labels, components, anchors);
        return new MassAnalysisReport(labels, masses);
    }

    /// <summary>
    ///     Analyzes pre-computed component labels shared from another pass.
    /// </summary>
    /// <param name="labels">Component labels indexed <c>[x, y]</c>, 0 for ocean.</param>
    /// <param name="components">(Size, SumX, SumY) triples per component, 0-based.</param>
    /// <param name="anchors">Resolved anchors, or null.</param>
    /// <returns>Analysis of every land mass in scan order of first cell.</returns>
    public static IReadOnlyList<MassAnalysisResult> Analyze(int[,] labels,
        IReadOnlyList<(long Size, double SumX, double SumY)> components,
        IReadOnlyList<ResolvedAnchor>? anchors)
    {
        return AnalyzeLabels(labels, components, anchors);
    }

    /// <summary>
    ///     Analyzes pre-computed component labels for significance. A mass is anchor-backed
    ///     when at least <see cref="AnchorBackedFraction" /> of its cells lie within the
    ///     extended footprint of some significant anchor, and significant when anchor-backed
    ///     or above the size floor.
    /// </summary>
    /// <param name="labels">Component labels indexed <c>[x, y]</c>, 0 for ocean.</param>
    /// <param name="components">(Size, SumX, SumY) triples per component, 0-based.</param>
    /// <param name="anchors">Resolved anchors, or null.</param>
    /// <returns>Analysis of every land mass keyed by label.</returns>
    public static IReadOnlyList<MassAnalysisResult> AnalyzeLabels(int[,] labels,
        IReadOnlyList<(long Size, double SumX, double SumY)> components,
        IReadOnlyList<ResolvedAnchor>? anchors)
    {
        var width = labels.GetLength(0);
        var height = labels.GetLength(1);

        // Significant anchors in resolved order, so backing is deterministic. An empty
        // effective anchor list keeps the grid pass to a plain land count.
        var significant = new List<int>();
        for (var i = 0; i < anchors?.Count; ++i)
        {
            if (anchors[i].Weight >= SignificantWeight) significant.Add(i);
        }
        var significantAnchors = significant.ConvertAll(index => anchors![index]);

        // Count, per mass, how many of its cells lie within some significant anchor's
        // extended footprint. The grid pass skips any cell beyond an anchor's precomputed
        // pixel extent, bounding the work near O(n + n-backing-cells * anchors).
        var totalLand = 0L;
        var backedCells = new long[components.Count];
        var extents = new (int MinX, int MaxX, int MinY, int MaxY)[significantAnchors.Count];
        for (var k = 0; k < significantAnchors.Count; ++k)
        {
            extents[k] = ExtentOf(significantAnchors[k], width, height);
        }

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var label = labels[x, y];
                if (label == 0) continue;

                ++totalLand;
                for (var k = 0; k < significantAnchors.Count; ++k)
                {
                    var (minX, maxX, minY, maxY) = extents[k];
                    if (x > maxX || y > maxY) continue;
                    if (BacksCell(x, y, significantAnchors[k], width, height))
                    {
                        ++backedCells[label - 1];
                        break;
                    }
                }
            }
        }

        var minFloorSize = Math.Max(FloorCells, (long)(FloorLandFraction * totalLand));
        var results = new List<MassAnalysisResult>(components.Count);
        for (var i = 0; i < components.Count; ++i)
        {
            var component = components[i];
            var size = component.Size;
            var isBacked = size > 0 && backedCells[i] * 2 >= size;
            var isSignificant = isBacked || size > minFloorSize;
            results.Add(new MassAnalysisResult(i + 1,
                (float)(component.SumX / size / width),
                (float)(component.SumY / size / height),
                size, isBacked, isSignificant));
        }

        return results;
    }

    /// <summary>
    ///     Labels the 4-connected components of the map's land cells and accumulates sizes
    ///     and coordinate sums.
    /// </summary>
    /// <param name="map">Terrain map; only the water flags are read.</param>
    /// <param name="components">Per-component (size, sum X, sum Y), populated.</param>
    /// <returns>Component labels indexed <c>[x, y]</c>; 0 for ocean.</returns>
    private static int[,] LabelComponents(TerrainMap map,
        out List<(long Size, double SumX, double SumY)> components)
    {
        var width = map.Width;
        var height = map.Height;
        var labels = new int[width, height];
        var queue = new Queue<(int X, int Y)>();
        components = new List<(long Size, double SumX, double SumY)>();

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (map.IsOcean[x, y] || labels[x, y] != 0) continue;

                var id = components.Count + 1;
                labels[x, y] = id;
                queue.Enqueue((x, y));
                long size = 0;
                double sumX = 0;
                double sumY = 0;
                while (queue.Count > 0)
                {
                    var (cx, cy) = queue.Dequeue();
                    ++size;
                    sumX += cx;
                    sumY += cy;
                    EnqueueIfLand(map, labels, queue, cx - 1, cy, width, height, id);
                    EnqueueIfLand(map, labels, queue, cx + 1, cy, width, height, id);
                    EnqueueIfLand(map, labels, queue, cx, cy - 1, width, height, id);
                    EnqueueIfLand(map, labels, queue, cx, cy + 1, width, height, id);
                }

                components.Add((size, sumX, sumY));
            }
        }

        return labels;
    }

    /// <summary>
    ///     Enqueues a neighbor and labels it when it is in-bounds land that is not yet
    ///     labeled.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="queue">Flood fill queue.</param>
    /// <param name="x">Neighbor X coordinate.</param>
    /// <param name="y">Neighbor Y coordinate.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <param name="id">Component label to apply.</param>
    private static void EnqueueIfLand(TerrainMap map, int[,] labels, Queue<(int X, int Y)> queue,
        int x, int y, int width, int height, int id)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (map.IsOcean[x, y] || labels[x, y] != 0) return;

        labels[x, y] = id;
        queue.Enqueue((x, y));
    }

    /// <summary>
    ///     Determines whether a cell lies within a significant anchor's extended footprint.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="anchor">Candidate backing anchor.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <returns>true if the cell is within the extended footprint, false otherwise.</returns>
    private static bool BacksCell(int x, int y, ResolvedAnchor anchor, int width, int height)
    {
        var u = (x + 0.5f) / width;
        var v = (y + 0.5f) / height;
        var dx = u - anchor.X;
        var dy = v - anchor.Y;
        var reach = anchor.Radius * AnchorFootprintFactor;
        return dx * dx + dy * dy <= reach * reach;
    }

    /// <summary>
    ///     Computes the inclusive pixel-space bounding extent where a cell can possibly lie
    ///     within an anchor's extended footprint, so the significance pass can skip cells
    ///     beyond it without the distance test.
    /// </summary>
    /// <param name="anchor">Anchor.</param>
    /// <param name="width">Footprint width.</param>
    /// <param name="height">Footprint height.</param>
    /// <returns>Inclusive extent in pixel coordinates, clamped to the footprint.</returns>
    private static (int MinX, int MaxX, int MinY, int MaxY) ExtentOf(ResolvedAnchor anchor,
        int width, int height)
    {
        var reach = anchor.Radius * AnchorFootprintFactor;
        var centerX = anchor.X * width;
        var centerY = anchor.Y * height;
        var minX = Math.Max(0, (int)MathF.Floor(centerX - reach * width) - 1);
        var maxX = Math.Min(width - 1, (int)MathF.Ceiling(centerX + reach * width) + 1);
        var minY = Math.Max(0, (int)MathF.Floor(centerY - reach * height) - 1);
        var maxY = Math.Min(height - 1, (int)MathF.Ceiling(centerY + reach * height) + 1);
        return (minX, maxX, minY, maxY);
    }
}
