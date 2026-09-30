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
///     Outcome of layout validation: the best-effort report plus the success flag and match
///     count used to pick the best resample attempt.
/// </summary>
public sealed class LayoutValidationResult
{
    /// <summary>
    ///     Best-effort layout report.
    /// </summary>
    public required LayoutReport Report { get; init; }

    /// <summary>
    ///     Whether every significant anchor matched a mass.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    ///     Number of significant anchors that matched a mass.
    /// </summary>
    public required int MatchedCount { get; init; }
}

/// <summary>
///     Validates that the shaped terrain honored the configured layout. Counts significant
///     land masses by 4-connectivity and requires each significant anchor to have a mass
///     centroid within a multiple of its radius. Layout validation never hard-fails a plan:
///     the caller resamples and accepts the best attempt with a report.
/// </summary>
public sealed class LayoutValidator
{
    /// <summary>
    ///     Minimum mass size as a fraction of total land for the mass to count as
    ///     significant.
    /// </summary>
    public const float SignificantMassFraction = 0.03f;

    /// <summary>
    ///     Maximum normalized centroid distance for an anchor, as a multiple of its radius.
    /// </summary>
    public const float MatchRadiusFactor = 1.5f;

    /// <summary>
    ///     Minimum anchor weight that participates in matching.
    /// </summary>
    public const float SignificantWeight = 0.5f;

    /// <summary>
    ///     Validates the shaped terrain against the resolved anchors.
    /// </summary>
    /// <param name="map">Shaped terrain map.</param>
    /// <param name="anchors">Resolved anchors.</param>
    /// <param name="warnings">Warnings accumulated during layout resolution.</param>
    /// <returns>Validation result.</returns>
    public LayoutValidationResult Validate(TerrainMap map,
        IReadOnlyList<ResolvedAnchor> anchors, IReadOnlyList<string> warnings)
    {
        var masses = SignificantMasses(map);
        var anchorReports = new List<LayoutAnchorReport>(anchors.Count);
        var matched = 0;

        foreach (var anchor in anchors)
        {
            var isSignificant = anchor.Weight >= SignificantWeight;
            var bestDistance = float.MaxValue;
            var bestIndex = -1;
            if (isSignificant)
            {
                for (var i = 0; i < masses.Count; ++i)
                {
                    var distance = NormalizedDistance(anchor, masses[i].CentroidX,
                        masses[i].CentroidY);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestIndex = i;
                    }
                }
            }

            var isMatched = bestIndex >= 0
                            && bestDistance <= MatchRadiusFactor * anchor.Radius;
            if (isMatched) ++matched;

            anchorReports.Add(new LayoutAnchorReport
            {
                Index = anchor.Index,
                X = anchor.X,
                Y = anchor.Y,
                Radius = anchor.Radius,
                Weight = anchor.Weight,
                Matched = isMatched,
                CentroidX = isMatched ? masses[bestIndex].CentroidX : float.NaN,
                CentroidY = isMatched ? masses[bestIndex].CentroidY : float.NaN
            });
        }

        var report = new LayoutReport
        {
            Anchors = anchorReports,
            MassCount = masses.Count,
            Warnings = warnings
        };

        return new LayoutValidationResult
        {
            Report = report,
            Success = report.AllSignificantAnchorsMatched,
            MatchedCount = matched
        };
    }

    /// <summary>
    ///     Computes the normalized Euclidean distance between an anchor and a normalized
    ///     centroid.
    /// </summary>
    /// <param name="anchor">Anchor.</param>
    /// <param name="centroidX">Centroid X in normalized coordinates.</param>
    /// <param name="centroidY">Centroid Y in normalized coordinates.</param>
    /// <returns>Normalized distance.</returns>
    private static float NormalizedDistance(ResolvedAnchor anchor, float centroidX, float centroidY)
    {
        var dx = centroidX - anchor.X;
        var dy = centroidY - anchor.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    ///     Labels the 4-connected land masses and returns those at least 3% of total land,
    ///     with their normalized centroids.
    /// </summary>
    /// <param name="map">Shaped terrain map.</param>
    /// <returns>Significant mass centroids.</returns>
    private static List<(float CentroidX, float CentroidY, long Size)> SignificantMasses(TerrainMap map)
    {
        var width = map.Width;
        var height = map.Height;
        var labels = new int[width, height];
        var queue = new Queue<(int X, int Y)>();
        var components = new List<(long Size, double SumX, double SumY)>();
        var totalLand = 0L;

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (map.IsOcean[x, y] || labels[x, y] != 0) continue;

                ++totalLand;
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

        var masses = new List<(float CentroidX, float CentroidY, long Size)>();
        if (totalLand == 0) return masses;

        var threshold = SignificantMassFraction * totalLand;
        foreach (var component in components)
        {
            if (component.Size < threshold) continue;
            masses.Add((
                (float)(component.SumX / component.Size / width),
                (float)(component.SumY / component.Size / height),
                component.Size));
        }

        return masses;
    }

    /// <summary>
    ///     Enqueues a neighbor and labels it when it is in-bounds land that is not yet
    ///     labeled.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="queue">Flood-fill queue.</param>
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
}
