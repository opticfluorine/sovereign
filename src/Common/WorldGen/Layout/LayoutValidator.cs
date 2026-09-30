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
///     Why a layout validation attempt failed.
/// </summary>
public enum LayoutFailureKind
{
    /// <summary>
    ///     The attempt succeeded.
    /// </summary>
    None,

    /// <summary>
    ///     One or more significant anchors matched no mass.
    /// </summary>
    UnmatchedAnchor,

    /// <summary>
    ///     Two or more significant anchors matched the same mass.
    /// </summary>
    SharedMass
}

/// <summary>
///     Outcome of layout validation: the best-effort report plus the success flag and match
///     counts used to pick the best resample attempt.
/// </summary>
public sealed class LayoutValidationResult
{
    /// <summary>
    ///     Best-effort layout report.
    /// </summary>
    public required LayoutReport Report { get; init; }

    /// <summary>
    ///     Whether every significant anchor matched a mass, respecting the configured
    ///     connectivity requirement.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    ///     Number of significant anchors that matched a mass after connectivity
    ///     invalidation.
    /// </summary>
    public required int MatchedCount { get; init; }

    /// <summary>
    ///     Number of distinct masses matched by significant anchors.
    /// </summary>
    public required int DistinctMatchedCount { get; init; }

    /// <summary>
    ///     Why the attempt failed, or <see cref="LayoutFailureKind.None" /> on success.
    /// </summary>
    public required LayoutFailureKind FailureKind { get; init; }
}

/// <summary>
///     Validates that the shaped terrain honored the configured layout. Counts significant
///     land masses by 4-connectivity and requires each significant anchor to have a mass
///     centroid within a multiple of its radius. The optional connectivity requirement also
///     requires significant anchors to match distinct masses. Layout validation never
///     hard-fails a plan by itself: the caller resamples and accepts the best attempt with a
///     report, unless the profile selects strict connectivity.
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
    ///     Ranks two validation attempts for best-attempt selection. Attempts are compared by
    ///     matched distinct count, then total matched count, then by how closely the number of
    ///     significant masses approximates the number of significant anchors.
    /// </summary>
    /// <param name="a">First attempt.</param>
    /// <param name="b">Second attempt.</param>
    /// <returns>Positive when a ranks above b, negative when b ranks above a, zero on a tie.</returns>
    public static int CompareAttempts(LayoutValidationResult a, LayoutValidationResult b)
    {
        var byDistinct = a.DistinctMatchedCount.CompareTo(b.DistinctMatchedCount);
        if (byDistinct != 0) return byDistinct;

        var byMatched = a.MatchedCount.CompareTo(b.MatchedCount);
        if (byMatched != 0) return byMatched;

        var aDelta = Math.Abs(a.Report.MassCount - a.Report.SignificantAnchorCount);
        var bDelta = Math.Abs(b.Report.MassCount - b.Report.SignificantAnchorCount);
        return bDelta.CompareTo(aDelta);
    }

    /// <summary>
    ///     Validates the shaped terrain against the resolved anchors.
    /// </summary>
    /// <param name="map">Shaped terrain map.</param>
    /// <param name="anchors">Resolved anchors.</param>
    /// <param name="warnings">Warnings accumulated during layout resolution.</param>
    /// <param name="connectivity">Connectivity requirement; defaults to
    ///     <see cref="LayoutConnectivity.None" />.</param>
    /// <returns>Validation result.</returns>
    public LayoutValidationResult Validate(TerrainMap map,
        IReadOnlyList<ResolvedAnchor> anchors, IReadOnlyList<string> warnings,
        LayoutConnectivity connectivity = LayoutConnectivity.None)
    {
        var masses = SignificantMasses(map);
        var count = anchors.Count;
        var matched = new bool[count];
        var sharedMass = new bool[count];
        var componentOf = new int[count];
        var centroidX = new float[count];
        var centroidY = new float[count];
        for (var i = 0; i < count; ++i)
        {
            componentOf[i] = -1;
            centroidX[i] = float.NaN;
            centroidY[i] = float.NaN;
        }

        for (var i = 0; i < count; ++i)
        {
            var anchor = anchors[i];
            if (anchor.Weight < SignificantWeight) continue;

            var bestDistance = float.MaxValue;
            var bestIndex = -1;
            for (var m = 0; m < masses.Count; ++m)
            {
                var distance = NormalizedDistance(anchor, masses[m].CentroidX,
                    masses[m].CentroidY);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = m;
                }
            }

            if (bestIndex < 0 || bestDistance > MatchRadiusFactor * anchor.Radius) continue;

            matched[i] = true;
            componentOf[i] = masses[bestIndex].ComponentId;
            centroidX[i] = masses[bestIndex].CentroidX;
            centroidY[i] = masses[bestIndex].CentroidY;
        }

        var sharedGroups = FindSharedMassGroups(componentOf, matched, connectivity);
        foreach (var group in sharedGroups)
        {
            foreach (var index in group)
            {
                matched[index] = false;
                sharedMass[index] = true;
            }
        }

        var anchorReports = new List<LayoutAnchorReport>(count);
        for (var i = 0; i < count; ++i)
        {
            var anchor = anchors[i];
            anchorReports.Add(new LayoutAnchorReport
            {
                Index = anchor.Index,
                X = anchor.X,
                Y = anchor.Y,
                Radius = anchor.Radius,
                Weight = anchor.Weight,
                Matched = matched[i],
                CentroidX = centroidX[i],
                CentroidY = centroidY[i],
                SharedMass = sharedMass[i]
            });
        }

        var matchedCount = 0;
        var allSignificantMatched = true;
        var distinct = new HashSet<int>();
        for (var i = 0; i < count; ++i)
        {
            if (anchors[i].Weight < SignificantWeight) continue;
            if (!matched[i])
            {
                allSignificantMatched = false;
                continue;
            }

            ++matchedCount;
            distinct.Add(componentOf[i]);
        }

        var report = new LayoutReport
        {
            Anchors = anchorReports,
            MassCount = masses.Count,
            Connectivity = connectivity,
            SharedMassGroups = sharedGroups,
            Warnings = warnings
        };

        return new LayoutValidationResult
        {
            Report = report,
            Success = allSignificantMatched,
            MatchedCount = matchedCount,
            DistinctMatchedCount = distinct.Count,
            FailureKind = allSignificantMatched
                ? LayoutFailureKind.None
                : sharedGroups.Count > 0
                    ? LayoutFailureKind.SharedMass
                    : LayoutFailureKind.UnmatchedAnchor
        };
    }

    /// <summary>
    ///     Finds the groups of significant anchors that matched the same mass. Groups are
    ///     returned only when the connectivity requirement forbids sharing, in anchor order.
    /// </summary>
    /// <param name="componentOf">Matched component id per anchor, or -1.</param>
    /// <param name="matched">Whether each anchor matched.</param>
    /// <param name="connectivity">Configured connectivity requirement.</param>
    /// <returns>Shared-mass anchor index groups.</returns>
    private static List<IReadOnlyList<int>> FindSharedMassGroups(int[] componentOf, bool[] matched,
        LayoutConnectivity connectivity)
    {
        var groups = new List<IReadOnlyList<int>>();
        if (connectivity == LayoutConnectivity.None) return groups;

        var byComponent = new Dictionary<int, List<int>>();
        for (var i = 0; i < componentOf.Length; ++i)
        {
            if (!matched[i]) continue;

            if (!byComponent.TryGetValue(componentOf[i], out var group))
            {
                group = new List<int>();
                byComponent[componentOf[i]] = group;
            }

            group.Add(i);
        }

        foreach (var group in byComponent.Values)
        {
            if (group.Count > 1) groups.Add(group);
        }

        groups.Sort((a, b) => a[0].CompareTo(b[0]));
        return groups;
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
    ///     with their component ids and normalized centroids.
    /// </summary>
    /// <param name="map">Shaped terrain map.</param>
    /// <returns>Significant masses.</returns>
    private static List<(int ComponentId, float CentroidX, float CentroidY, long Size)>
        SignificantMasses(TerrainMap map)
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

        var masses = new List<(int ComponentId, float CentroidX, float CentroidY, long Size)>();
        if (totalLand == 0) return masses;

        var threshold = SignificantMassFraction * totalLand;
        for (var i = 0; i < components.Count; ++i)
        {
            var component = components[i];
            if (component.Size < threshold) continue;
            masses.Add((
                i + 1,
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
