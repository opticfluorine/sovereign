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
using System.Threading.Tasks;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Caves;

/// <summary>
///     Result of generating one cave level.
/// </summary>
public sealed class CaveLevelResult
{
    /// <summary>
    ///     Carved data of the level.
    /// </summary>
    public required CaveLevelMap Map { get; init; }

    /// <summary>
    ///     Number of open-cell components before connectivity repair.
    /// </summary>
    public required int ComponentsBeforeRepair { get; init; }

    /// <summary>
    ///     Number of open-cell components after connectivity repair.
    /// </summary>
    public required int ComponentsAfterRepair { get; init; }

    /// <summary>
    ///     Total cells carved by repair corridors.
    /// </summary>
    public required int RepairCorridorCells { get; init; }

    /// <summary>
    ///     Whether the repair corridor budget was exhausted before the level was fully
    ///     connected.
    /// </summary>
    public required bool RepairBudgetExceeded { get; init; }

    /// <summary>
    ///     Fraction of the level area that is open after exclusion and repair.
    /// </summary>
    public required double RealizedOpenFraction { get; init; }
}

/// <summary>
///     Generates one cave level: a near-flat floor field, a fractal Worley porosity mask,
///     the water-proximity exclusion, and union-find connectivity repair. Floors vary at
///     most two blocks from the base and never step more than one between adjacent open
///     cells, so no rapid vertical oscillation is possible by construction.
/// </summary>
public sealed class CaveLevelGenerator
{
    /// <summary>
    ///     Wavelength of the base octave of the floor field, in blocks.
    /// </summary>
    private const float FloorFieldWavelength = 400f;

    /// <summary>
    ///     Octave count of the floor field fBm.
    /// </summary>
    private const int FloorFieldOctaves = 2;

    /// <summary>
    ///     Maximum floor deviation from the base floor Z, in blocks.
    /// </summary>
    private const int MaxFloorDeviation = 2;

    /// <summary>
    ///     Wavelength of the base octave of the Worley porosity field, in blocks.
    /// </summary>
    private const float WorleyBaseWavelength = 128f;

    /// <summary>
    ///     Octave count of the fractal Worley field.
    /// </summary>
    private const int WorleyOctaves = 2;

    /// <summary>
    ///     Histogram bin count used to calibrate the Worley open threshold.
    /// </summary>
    private const int WorleyHistogramBins = 1024;

    /// <summary>
    ///     Upper bound of the normalized Worley F2-F1 value range.
    /// </summary>
    private const float WorleyFieldValueMax = 1.5f;

    /// <summary>
    ///     Bank exclusion radius in cells around rivers, lakes, banks, and ocean.
    /// </summary>
    private const int WaterBankRadius = 4;

    /// <summary>
    ///     Minimum blocks of rock required between a cave ceiling and a surface water floor.
    /// </summary>
    private const int CeilingClearance = 3;

    /// <summary>
    ///     Minimum blocks between a lake surface and a cave floor; shallower columns under
    ///     lakes are excluded outright.
    /// </summary>
    private const int MinLakeColumnDepth = 8;

    /// <summary>
    ///     Maximum repair corridor length as a fraction of the level's open-cell count.
    /// </summary>
    private const double MaxRepairBudgetFraction = 0.05;

    /// <summary>
    ///     Maximum sidestep distance for greedy corridor deviation around blocked cells.
    /// </summary>
    private const int MaxCorridorDeviation = 3;

    /// <summary>
    ///     Maximum cells a corridor walks along a water-exclusion barrier while looking for
    ///     a free step forward. Sized to walk around the rim of a large lake.
    /// </summary>
    private const int MaxWallFollow = 512;

    /// <summary>
    ///     Maximum number of already-connected components tried when attaching one
    ///     component during connectivity repair.
    /// </summary>
    private const int MaxRepairCandidates = 3;

    private int width;
    private int height;

    /// <summary>
    ///     Generates one cave level.
    /// </summary>
    /// <param name="level">Level configuration from the profile.</param>
    /// <param name="terrain">Terrain map with heights and water flags populated.</param>
    /// <param name="options">Cave generation options.</param>
    /// <param name="seed">Sub-seed for this level.</param>
    /// <returns>The generated level.</returns>
    public CaveLevelResult Generate(CaveLevel level, TerrainMap terrain, CaveOptions options,
        ulong seed)
    {
        width = terrain.Width;
        height = terrain.Height;

        var floorZ = BuildFloorField(level, seed);
        var worley = BuildWorleyField(seed);
        var open = BuildOpenMask(worley, CalibrateThreshold(worley, options.Porosity));
        var excluded = BuildWaterExclusion(floorZ, level, terrain);
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (excluded[x, y]) open[x, y] = false;
            }
        }

        var (componentsBefore, labels) = LabelComponents(open);

        var openCount = CountOpen(open);
        var budget = (int)(openCount * MaxRepairBudgetFraction);
        var (repairCells, componentsAfter, budgetExceeded) =
            RepairConnectivity(open, floorZ, excluded, labels, componentsBefore,
                options.MinTunnelWidth, level.FloorZ, budget);

        return new CaveLevelResult
        {
            Map = new CaveLevelMap
            {
                Width = width,
                Height = height,
                BaseFloorZ = level.FloorZ,
                Headroom = level.Headroom,
                Open = open,
                FloorZ = floorZ,
                CarveHeight = BuildCarveHeights(open, level.Headroom),
                Worley = QuantizeWorley(worley),
                WaterExcluded = excluded
            },
            ComponentsBeforeRepair = componentsBefore,
            ComponentsAfterRepair = componentsAfter,
            RepairCorridorCells = repairCells,
            RepairBudgetExceeded = budgetExceeded,
            RealizedOpenFraction = CountOpen(open) / (double)(width * height)
        };
    }

    /// <summary>
    ///     Builds the floor field: base floor Z plus a low-frequency fBm offset clamped to
    ///     two blocks.
    /// </summary>
    /// <param name="level">Level configuration.</param>
    /// <param name="seed">Level sub-seed.</param>
    /// <returns>Floor Z per cell.</returns>
    private int[,] BuildFloorField(CaveLevel level, ulong seed)
    {
        var noise = new SeededNoise(SeedDerivation.DeriveSubSeed(seed, "FloorField"));
        var floorZ = new int[width, height];
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var lowFreq = Math.Clamp(
                    noise.Fbm(x, y, FloorFieldWavelength, FloorFieldOctaves), -1f, 1f);
                floorZ[x, y] = level.FloorZ + (int)MathF.Round(MaxFloorDeviation * lowFreq);
            }
        });

        return floorZ;
    }

    /// <summary>
    ///     Builds the fractal Worley F2-F1 field at block resolution.
    /// </summary>
    /// <param name="seed">Level sub-seed.</param>
    /// <returns>Normalized Worley F2-F1 value per cell.</returns>
    private float[,] BuildWorleyField(ulong seed)
    {
        return FractalWorley.Field(width, height, WorleyBaseWavelength, WorleyOctaves,
            SeedDerivation.DeriveSubSeed(seed, "Worley"));
    }

    /// <summary>
    ///     Calibrates the Worley open threshold so that the configured porosity fraction of
    ///     cells falls below it.
    /// </summary>
    /// <param name="worley">Worley F2-F1 field.</param>
    /// <param name="porosity">Configured porosity.</param>
    /// <returns>Open threshold.</returns>
    private float CalibrateThreshold(float[,] worley, double porosity)
    {
        var histogram = new int[WorleyHistogramBins];
        var total = (long)width * height;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                ++histogram[BinOf(worley[x, y])];
            }
        }

        var target = (long)(total * porosity);
        var cumulative = 0L;
        for (var bin = 0; bin < WorleyHistogramBins; ++bin)
        {
            var next = cumulative + histogram[bin];
            if (next < target)
            {
                cumulative = next;
                continue;
            }

            var inBin = histogram[bin];
            var fraction = inBin > 0 ? (double)(target - cumulative) / inBin : 0.0;
            return (bin + (float)fraction) / WorleyHistogramBins * WorleyFieldValueMax;
        }

        return WorleyFieldValueMax;
    }

    /// <summary>
    ///     Computes the histogram bin of a Worley value.
    /// </summary>
    /// <param name="value">Worley F2-F1 value.</param>
    /// <returns>Histogram bin index.</returns>
    private static int BinOf(float value)
    {
        var bin = (int)(value / WorleyFieldValueMax * WorleyHistogramBins);
        return Math.Clamp(bin, 0, WorleyHistogramBins - 1);
    }

    /// <summary>
    ///     Builds the porosity mask from the calibrated threshold.
    /// </summary>
    /// <param name="worley">Worley F2-F1 field.</param>
    /// <param name="threshold">Calibrated open threshold.</param>
    /// <returns>Porosity mask.</returns>
    private bool[,] BuildOpenMask(float[,] worley, float threshold)
    {
        var open = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                open[x, y] = worley[x, y] < threshold;
            }
        }

        return open;
    }

    /// <summary>
    ///     Builds the water-proximity exclusion: cells whose ceiling would come within the
    ///     clearance of a surface water floor are kept solid, as are shallow columns under
    ///     lakes. The mask covers every cell regardless of the porosity mask, so repair
    ///     corridors can never route through excluded rock.
    /// </summary>
    /// <param name="floorZ">Floor field.</param>
    /// <param name="level">Level configuration.</param>
    /// <param name="terrain">Terrain map.</param>
    /// <returns>The water exclusion mask.</returns>
    private bool[,] BuildWaterExclusion(int[,] floorZ, CaveLevel level, TerrainMap terrain)
    {
        var nearWater = BuildNearWaterMask(terrain);
        var excluded = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var ceilingZ = floorZ[x, y] + level.Headroom;
                var excludedHere = terrain.IsLake[x, y]
                    && terrain.LakeSurfaceZ[x, y] - floorZ[x, y] < MinLakeColumnDepth;
                excludedHere |= nearWater[x, y]
                                && terrain.Heights[x, y] - ceilingZ < CeilingClearance;
                excluded[x, y] = excludedHere;
            }
        }

        return excluded;
    }

    /// <summary>
    ///     Builds the near-water mask: river, lake, and bank cells dilated by the bank
    ///     radius. Ocean is not a seed: the card's rule keys on surface water channels, and
    ///     seafloor depth is handled by the surface carving stages.
    /// </summary>
    /// <param name="terrain">Terrain map.</param>
    /// <returns>Near-water mask.</returns>
    private bool[,] BuildNearWaterMask(TerrainMap terrain)
    {
        var mask = new bool[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                mask[x, y] = terrain.IsRiver[x, y] || terrain.IsLake[x, y]
                             || terrain.IsBank[x, y];
            }
        }

        for (var pass = 0; pass < WaterBankRadius; ++pass)
        {
            var next = new bool[width, height];
            Parallel.For(0, height, y =>
            {
                for (var x = 0; x < width; ++x)
                {
                    next[x, y] = mask[x, y]
                                 || x > 0 && mask[x - 1, y]
                                 || x < width - 1 && mask[x + 1, y]
                                 || y > 0 && mask[x, y - 1]
                                 || y < height - 1 && mask[x, y + 1];
                }
            });
            mask = next;
        }

        return mask;
    }

    /// <summary>
    ///     Labels the 4-connected components of the open mask with union-find.
    /// </summary>
    /// <param name="open">Open mask.</param>
    /// <returns>Component count and per-cell component labels (-1 for solid cells).</returns>
    private (int Count, int[] Labels) LabelComponents(bool[,] open)
    {
        var total = width * height;
        var uf = new UnionFind(total);
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (!open[x, y]) continue;

                var index = y * width + x;
                if (x > 0 && open[x - 1, y]) uf.Union(index, index - 1);
                if (y > 0 && open[x, y - 1]) uf.Union(index, index - width);
            }
        }

        var labels = new int[total];
        var remap = new Dictionary<int, int>();
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var index = y * width + x;
                if (!open[x, y])
                {
                    labels[index] = -1;
                    continue;
                }

                var root = uf.Find(index);
                if (!remap.TryGetValue(root, out var label))
                {
                    label = remap.Count;
                    remap[root] = label;
                }

                labels[index] = label;
            }
        }

        return (remap.Count, labels);
    }

    /// <summary>
    ///     Repairs connectivity by carving L-shaped corridors between component centroids
    ///     until one component remains or the repair budget is exhausted. Each component
    ///     tries the nearest centroids among the components already merged into the trunk;
    ///     unmerged components are retried in further passes as the trunk grows.
    /// </summary>
    /// <param name="open">Open mask, updated in place.</param>
    /// <param name="floorZ">Floor field, updated in place for carved cells.</param>
    /// <param name="excluded">Water exclusion mask; corridors never cross it.</param>
    /// <param name="labels">Component labels from the pre-repair labeling.</param>
    /// <param name="componentCount">Number of components before repair.</param>
    /// <param name="tunnelWidth">Corridor carve width in blocks.</param>
    /// <param name="baseFloorZ">Base floor Z of the level.</param>
    /// <param name="budget">Maximum total corridor cells.</param>
    /// <returns>Total corridor cells, final component count, and whether the budget was
    ///     exhausted.</returns>
    private (int RepairCells, int ComponentsAfter, bool BudgetExceeded) RepairConnectivity(
        bool[,] open, int[,] floorZ, bool[,] excluded, int[] labels, int componentCount,
        int tunnelWidth, int baseFloorZ, int budget)
    {
        if (componentCount <= 1) return (0, componentCount, false);

        var members = CollectMembers(labels, componentCount);
        var order = new List<int>(members.Keys);
        order.Sort((a, b) => members[b].Count.CompareTo(members[a].Count));

        var repairCells = 0;
        var merged = new List<int> { order[0] };
        var unmerged = new List<int>(order.Skip(1));
        var budgetExceeded = false;
        var progress = true;

        while (progress && !budgetExceeded && unmerged.Count > 0)
        {
            progress = false;
            var remaining = new List<int>();
            foreach (var component in unmerged)
            {
                if (budgetExceeded)
                {
                    remaining.Add(component);
                    continue;
                }

                var attached = false;
                foreach (var host in NearestFirst(merged, members, component))
                {
                    var carved = CarveCorridor(NearestToCentroid(members[component]),
                        NearestToCentroid(members[host]), tunnelWidth, open, floorZ, excluded,
                        baseFloorZ);
                    if (carved < 0) continue;

                    repairCells += carved;
                    merged.Add(component);
                    attached = true;
                    progress = true;
                    if (repairCells > budget) budgetExceeded = true;
                    break;
                }

                if (!attached) remaining.Add(component);
            }

            unmerged = remaining;
        }

        var (componentsAfter, _) = LabelComponents(open);
        return (repairCells, componentsAfter, budgetExceeded);
    }

    /// <summary>
    ///     Collects the open cell coordinates of each component.
    /// </summary>
    /// <param name="labels">Component labels.</param>
    /// <param name="componentCount">Number of components.</param>
    /// <returns>Cells per component label.</returns>
    private Dictionary<int, List<(int X, int Y)>> CollectMembers(int[] labels,
        int componentCount)
    {
        var members = new Dictionary<int, List<(int X, int Y)>>(componentCount);
        for (var i = 0; i < componentCount; ++i) members[i] = new List<(int X, int Y)>();

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var label = labels[y * width + x];
                if (label >= 0) members[label].Add((x, y));
            }
        }

        return members;
    }

    /// <summary>
    ///     Orders the merged components by centroid distance to the given component: the
    ///     nearest merged hosts plus the trunk itself, so a pocket behind small components
    ///     still attempts a direct corridor to the trunk.
    /// </summary>
    /// <param name="merged">Component labels already merged into the trunk; the first entry
    ///     is the trunk.</param>
    /// <param name="members">Cells per component.</param>
    /// <param name="component">Component to attach.</param>
    /// <returns>Host component labels, nearest first.</returns>
    private static IEnumerable<int> NearestFirst(List<int> merged,
        Dictionary<int, List<(int X, int Y)>> members, int component)
    {
        var (cx, cy) = Centroid(members[component]);
        var ranked = new List<(double Distance, int Label)>(merged.Count);
        foreach (var host in merged)
        {
            var (hx, hy) = Centroid(members[host]);
            var dx = hx - cx;
            var dy = hy - cy;
            ranked.Add((dx * dx + dy * dy, host));
        }

        ranked.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        var candidates = new List<int>();
        foreach (var (_, label) in ranked)
        {
            if (candidates.Count >= MaxRepairCandidates - 1) break;
            candidates.Add(label);
        }

        // The trunk is always tried last.
        if (!candidates.Contains(merged[0])) candidates.Add(merged[0]);
        return candidates;
    }

    /// <summary>
    ///     Computes the centroid of a component.
    /// </summary>
    /// <param name="cells">Cells of the component.</param>
    /// <returns>Centroid coordinates.</returns>
    private static (double X, double Y) Centroid(List<(int X, int Y)> cells)
    {
        double sx = 0;
        double sy = 0;
        foreach (var (x, y) in cells)
        {
            sx += x;
            sy += y;
        }

        return (sx / cells.Count, sy / cells.Count);
    }

    /// <summary>
    ///     Finds the cell of a component nearest its centroid.
    /// </summary>
    /// <param name="cells">Cells of the component.</param>
    /// <returns>Cell coordinates.</returns>
    private static (int X, int Y) NearestToCentroid(List<(int X, int Y)> cells)
    {
        var (cx, cy) = Centroid(cells);
        var best = cells[0];
        var bestDistance = double.MaxValue;
        foreach (var (x, y) in cells)
        {
            var dx = x - cx;
            var dy = y - cy;
            var distance = dx * dx + dy * dy;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = (x, y);
            }
        }

        return best;
    }

    /// <summary>
    ///     Carves an L-shaped corridor between two cells: horizontal leg first, then vertical
    ///     leg, deviating greedily around water-excluded cells. The route is validated
    ///     against the carve width before any cell is opened. Carved floors are re-leveled.
    /// </summary>
    /// <param name="from">Start cell.</param>
    /// <param name="to">End cell.</param>
    /// <param name="tunnelWidth">Corridor carve width in blocks.</param>
    /// <param name="open">Open mask, updated in place.</param>
    /// <param name="floorZ">Floor field, updated in place.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <param name="baseFloorZ">Base floor Z of the level.</param>
    /// <returns>Number of newly carved cells, or -1 if the corridor could not be routed.</returns>
    private int CarveCorridor((int X, int Y) from, (int X, int Y) to, int tunnelWidth,
        bool[,] open, int[,] floorZ, bool[,] excluded, int baseFloorZ)
    {
        var horizontal = RouteLeg(from, to, horizontalFirst: true, tunnelWidth, excluded);
        if (horizontal is null) return -1;

        var vertical = RouteLeg(horizontal[^1], to, horizontalFirst: false, tunnelWidth,
            excluded);
        if (vertical is null) return -1;

        var path = new List<(int X, int Y)>(horizontal);
        for (var i = 1; i < vertical.Count; ++i) path.Add(vertical[i]);

        foreach (var cell in path)
        {
            if (!CanStamp(cell, tunnelWidth, excluded)) return -1;
        }

        var carved = new List<(int X, int Y)>();
        foreach (var cell in path)
        {
            Stamp(cell, tunnelWidth, open, carved);
        }

        EnforceFloorSlope(carved, open, floorZ, baseFloorZ);
        return carved.Count;
    }

    /// <summary>
    ///     Routes one corridor leg with greedy deviation around blocked cells: on a blocked
    ///     step, the path sidesteps perpendicular by up to three cells and continues along
    ///     the sidestepped line.
    /// </summary>
    /// <param name="from">Leg start cell.</param>
    /// <param name="to">Leg end cell.</param>
    /// <param name="horizontalFirst">Whether the leg advances along X first.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>The routed cells in walk order, or null if the leg could not be routed.</returns>
    private List<(int X, int Y)>? RouteLeg((int X, int Y) from, (int X, int Y) to,
        bool horizontalFirst, int tunnelWidth, bool[,] excluded)
    {
        var cells = new List<(int X, int Y)> { from };

        bool Walk(bool horizontal, int targetPrimary)
        {
            while (true)
            {
                var (x, y) = cells[^1];
                var primary = horizontal ? x : y;
                if (primary == targetPrimary) return true;

                var nextPrimary = primary + Math.Sign(targetPrimary - primary);
                var (nx, ny) = horizontal ? (nextPrimary, y) : (x, nextPrimary);
                if (BlockPassable(nx, ny, tunnelWidth, excluded))
                {
                    cells.Add((nx, ny));
                    continue;
                }

                if (!Deviate(cells, horizontal, nextPrimary, tunnelWidth, excluded)) return false;
            }
        }

        if (horizontalFirst)
        {
            if (!Walk(true, to.X)) return null;
            if (!Walk(false, to.Y)) return null;
        }
        else
        {
            if (!Walk(false, to.Y)) return null;
            if (!Walk(true, to.X)) return null;
        }

        return cells;
    }

    /// <summary>
    ///     Sidesteps a blocked step perpendicular to the leg: first by up to three cells
    ///     onto a line just past the blocked cell, then, failing that, by walking along the
    ///     exclusion barrier for up to the wall-follow bound until a free step forward
    ///     appears.
    /// </summary>
    /// <param name="cells">Routed cells; appended in place on success.</param>
    /// <param name="horizontal">Whether the leg advances along X.</param>
    /// <param name="blockedPrimary">Primary coordinate of the blocked cell.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if the path was deviated past the blocked cell, false otherwise.</returns>
    private bool Deviate(List<(int X, int Y)> cells, bool horizontal, int blockedPrimary,
        int tunnelWidth, bool[,] excluded)
    {
        if (Sidestep(cells, horizontal, blockedPrimary, tunnelWidth, excluded)) return true;
        return WallFollow(cells, horizontal, blockedPrimary, tunnelWidth, excluded);
    }

    /// <summary>
    ///     Sidesteps a blocked step perpendicular to the leg by up to three cells and moves
    ///     the path onto the sidestepped line just past the blocked cell.
    /// </summary>
    /// <param name="cells">Routed cells; appended in place on success.</param>
    /// <param name="horizontal">Whether the leg advances along X.</param>
    /// <param name="blockedPrimary">Primary coordinate of the blocked cell.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if the path was deviated past the blocked cell, false otherwise.</returns>
    private bool Sidestep(List<(int X, int Y)> cells, bool horizontal,
        int blockedPrimary, int tunnelWidth, bool[,] excluded)
    {
        var (x, y) = cells[^1];
        var secondary = horizontal ? y : x;

        foreach (var side in new[] { 1, -1 })
        {
            for (var dev = 1; dev <= MaxCorridorDeviation; ++dev)
            {
                var shifted = secondary + side * dev;
                if (!BlockPassable(horizontal ? blockedPrimary : shifted,
                        horizontal ? shifted : blockedPrimary, tunnelWidth, excluded))
                {
                    continue;
                }

                var free = true;
                for (var s = secondary + side; side > 0 ? s <= shifted : s >= shifted; s += side)
                {
                    if (BlockPassable(horizontal ? x : s, horizontal ? s : y, tunnelWidth,
                            excluded)) continue;

                    free = false;
                    break;
                }

                if (!free) continue;

                for (var s = secondary + side; side > 0 ? s <= shifted : s >= shifted; s += side)
                {
                    cells.Add(horizontal ? (x, s) : (s, y));
                }

                cells.Add(horizontal ? (blockedPrimary, shifted) : (shifted, blockedPrimary));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Walks the path along the exclusion barrier perpendicular to the leg, in one
    ///     direction then the other, until a free step forward appears or the bound is
    ///     exhausted. The barrier is never crossed: every walked cell is water-clear.
    /// </summary>
    /// <param name="cells">Routed cells; appended in place on success.</param>
    /// <param name="horizontal">Whether the leg advances along X.</param>
    /// <param name="blockedPrimary">Primary coordinate of the blocked cell.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if a free step forward was found past the blocked cell, false
    ///     otherwise.</returns>
    private bool WallFollow(List<(int X, int Y)> cells, bool horizontal, int blockedPrimary,
        int tunnelWidth, bool[,] excluded)
    {
        var (x, y) = cells[^1];
        var secondary = horizontal ? y : x;

        foreach (var side in new[] { 1, -1 })
        {
            var probe = cells[^1];
            var walked = new List<(int X, int Y)>();
            var found = false;
            for (var k = 1; k <= MaxWallFollow; ++k)
            {
                var shifted = secondary + side * k;
                var along = horizontal ? (x, shifted) : (shifted, y);
                if (!BlockPassable(along.Item1, along.Item2, tunnelWidth, excluded)) break;

                walked.Add(along);
                var forward = horizontal ? (blockedPrimary, shifted) : (shifted, blockedPrimary);
                if (BlockPassable(forward.Item1, forward.Item2, tunnelWidth, excluded))
                {
                    probe = forward;
                    found = true;
                    break;
                }
            }

            if (!found) continue;

            cells.AddRange(walked);
            cells.Add(probe);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Determines whether a cell may be carved: in bounds and not water-excluded.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if the cell may be carved, false otherwise.</returns>
    private bool Passable(int x, int y, bool[,] excluded)
    {
        return x >= 0 && y >= 0 && x < width && y < height && !excluded[x, y];
    }

    /// <summary>
    ///     Determines whether a tunnel-width block anchored at the cell is in bounds and
    ///     water-clear, matching the carve stamping.
    /// </summary>
    /// <param name="x">Anchor cell X coordinate.</param>
    /// <param name="y">Anchor cell Y coordinate.</param>
    /// <param name="tunnelWidth">Carve width in blocks.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if the block may be carved, false otherwise.</returns>
    private bool BlockPassable(int x, int y, int tunnelWidth, bool[,] excluded)
    {
        return CanStamp((x, y), tunnelWidth, excluded);
    }

    /// <summary>
    ///     Determines whether a tunnel-width block anchored at the cell may be carved: the
    ///     in-bounds cells of the block must be water-clear. Blocks at the map edge are
    ///     clipped rather than rejected.
    /// </summary>
    /// <param name="cell">Anchor cell.</param>
    /// <param name="tunnelWidth">Carve width in blocks.</param>
    /// <param name="excluded">Water exclusion mask.</param>
    /// <returns>true if the block may be carved, false otherwise.</returns>
    private bool CanStamp((int X, int Y) cell, int tunnelWidth, bool[,] excluded)
    {
        for (var dy = 0; dy < tunnelWidth; ++dy)
        {
            for (var dx = 0; dx < tunnelWidth; ++dx)
            {
                var x = cell.X + dx;
                var y = cell.Y + dy;
                if (x >= width || y >= height) continue;
                if (excluded[x, y]) return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Stamps a tunnel-width block anchored at the cell, clipped to the map, marking
    ///     newly carved cells.
    /// </summary>
    /// <param name="cell">Anchor cell.</param>
    /// <param name="tunnelWidth">Carve width in blocks.</param>
    /// <param name="open">Open mask.</param>
    /// <param name="carved">List of newly carved cells, appended in place.</param>
    private void Stamp((int X, int Y) cell, int tunnelWidth, bool[,] open,
        List<(int X, int Y)> carved)
    {
        for (var dy = 0; dy < tunnelWidth; ++dy)
        {
            for (var dx = 0; dx < tunnelWidth; ++dx)
            {
                var x = cell.X + dx;
                var y = cell.Y + dy;
                if (x >= width || y >= height) continue;
                if (open[x, y]) continue;

                open[x, y] = true;
                carved.Add((x, y));
            }
        }
    }

    /// <summary>
    ///     Enforces the floor slope rule over the carved cells: an open cell never lies more
    ///     than one block below an adjacent open cell; the lower cell is raised, never the
    ///     higher lowered, and floors stay within the deviation of the base.
    /// </summary>
    /// <param name="seedCells">Cells to seed the propagation from.</param>
    /// <param name="open">Open mask.</param>
    /// <param name="floorZ">Floor field, updated in place.</param>
    /// <param name="baseFloorZ">Base floor Z of the level.</param>
    private void EnforceFloorSlope(List<(int X, int Y)> seedCells, bool[,] open, int[,] floorZ,
        int baseFloorZ)
    {
        FloorSlopePass.Apply(seedCells, open, floorZ, width, height, baseFloorZ,
            MaxFloorDeviation);
    }

    /// <summary>
    ///     Enumerates the in-bounds 4-neighbors of a cell.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Neighbor coordinates.</returns>
    private IEnumerable<(int X, int Y)> Neighbors(int x, int y)
    {
        if (x > 0) yield return (x - 1, y);
        if (y > 0) yield return (x, y - 1);
        if (x < width - 1) yield return (x + 1, y);
        if (y < height - 1) yield return (x, y + 1);
    }

    /// <summary>
    ///     Counts open cells.
    /// </summary>
    /// <param name="open">Open mask.</param>
    /// <returns>Open cell count.</returns>
    private int CountOpen(bool[,] open)
    {
        var count = 0;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (open[x, y]) ++count;
            }
        }

        return count;
    }

    /// <summary>
    ///     Builds the carve height array: the level headroom above every open floor cell.
    /// </summary>
    /// <param name="open">Open mask.</param>
    /// <param name="headroom">Level headroom in blocks.</param>
    /// <returns>Carve height per cell; zero for solid cells.</returns>
    private byte[,] BuildCarveHeights(bool[,] open, int headroom)
    {
        var carveHeight = new byte[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (open[x, y]) carveHeight[x, y] = (byte)headroom;
            }
        }

        return carveHeight;
    }

    /// <summary>
    ///     Quantizes the Worley field to bytes for the cave preview's corridor-versus-chamber
    ///     shading.
    /// </summary>
    /// <param name="worley">Worley F2-F1 field.</param>
    /// <returns>Quantized field.</returns>
    private byte[,] QuantizeWorley(float[,] worley)
    {
        var quantized = new byte[width, height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                quantized[x, y] = (byte)Math.Clamp(
                    worley[x, y] / WorleyFieldValueMax * 255f, 0f, 255f);
            }
        }

        return quantized;
    }

    /// <summary>
    ///     Union-find over flat cell indices with path halving and union by size.
    /// </summary>
    private sealed class UnionFind
    {
        private readonly int[] parent;
        private readonly int[] size;

        /// <summary>
        ///     Creates a union-find over the given number of elements.
        /// </summary>
        /// <param name="n">Element count.</param>
        public UnionFind(int n)
        {
            parent = new int[n];
            size = new int[n];
            for (var i = 0; i < n; ++i)
            {
                parent[i] = i;
                size[i] = 1;
            }
        }

        /// <summary>
        ///     Finds the root of an element with path halving.
        /// </summary>
        /// <param name="x">Element index.</param>
        /// <returns>Root index.</returns>
        public int Find(int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }

            return x;
        }

        /// <summary>
        ///     Unions two elements by size.
        /// </summary>
        /// <param name="a">First element.</param>
        /// <param name="b">Second element.</param>
        public void Union(int a, int b)
        {
            var ra = Find(a);
            var rb = Find(b);
            if (ra == rb) return;

            if (size[ra] < size[rb]) (ra, rb) = (rb, ra);
            parent[rb] = ra;
            size[ra] += size[rb];
        }
    }
}

/// <summary>
///     Floor slope post-pass shared by cave generation stages: raises lower open cells
///     until adjacent open cells differ by at most one block, never lowering a cell or
///     exceeding the level's floor deviation.
/// </summary>
internal static class FloorSlopePass
{
    /// <summary>
    ///     Applies the slope pass seeded from the given cells.
    /// </summary>
    /// <param name="seedCells">Cells to seed the propagation from.</param>
    /// <param name="open">Open mask.</param>
    /// <param name="floorZ">Floor field, updated in place.</param>
    /// <param name="width">Level width in blocks.</param>
    /// <param name="height">Level height in blocks.</param>
    /// <param name="baseFloorZ">Base floor Z of the level.</param>
    /// <param name="maxDeviation">Maximum floor deviation from the base, in blocks.</param>
    public static void Apply(List<(int X, int Y)> seedCells, bool[,] open, int[,] floorZ,
        int width, int height, int baseFloorZ, int maxDeviation)
    {
        var queue = new Queue<(int X, int Y)>(seedCells);
        var ceiling = baseFloorZ + maxDeviation;
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            foreach (var (nx, ny) in Neighbors(x, y, width, height))
            {
                if (!open[nx, ny]) continue;
                if (floorZ[nx, ny] >= floorZ[x, y] - 1) continue;

                floorZ[nx, ny] = Math.Min(floorZ[x, y] - 1, ceiling);
                queue.Enqueue((nx, ny));
            }
        }
    }

    /// <summary>
    ///     Enumerates the in-bounds 4-neighbors of a cell.
    /// </summary>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <param name="width">Level width in blocks.</param>
    /// <param name="height">Level height in blocks.</param>
    /// <returns>Neighbor coordinates.</returns>
    private static IEnumerable<(int X, int Y)> Neighbors(int x, int y, int width, int height)
    {
        if (x > 0) yield return (x - 1, y);
        if (y > 0) yield return (x, y - 1);
        if (x < width - 1) yield return (x + 1, y);
        if (y < height - 1) yield return (x, y + 1);
    }
}

/// <summary>
///     Deterministic fractal two-dimensional Worley noise. Each grid cell carries one
///     feature point at a seeded jittered position; the sampled value is F2-F1 over the
///     3x3 neighborhood, normalized by the cell size. Feature points are cached per grid
///     row so the field pass evaluates each point's jitter exactly once.
/// </summary>
internal static class FractalWorley
{
    /// <summary>
    ///     Amplitude multipliers of successive octaves.
    /// </summary>
    private const float OctaveFalloff = 0.5f;

    /// <summary>
    ///     Uniform variate scale for 53-bit SplitMix64 draws.
    /// </summary>
    private const double UniformScale = 1.0 / 9007199254740992.0;

    /// <summary>
    ///     Builds the full fractal Worley F2-F1 field over a rectangle.
    /// </summary>
    /// <param name="width">Field width in blocks.</param>
    /// <param name="height">Field height in blocks.</param>
    /// <param name="wavelength">Wavelength of the base octave in blocks.</param>
    /// <param name="octaves">Number of octaves to sum.</param>
    /// <param name="seed">Seed for the feature point jitter.</param>
    /// <returns>Weighted F2-F1 value per cell in approximately [0, 1.5].</returns>
    public static float[,] Field(int width, int height, float wavelength, int octaves,
        ulong seed)
    {
        var field = new float[width, height];
        var amplitude = 1f;
        var norm = 0f;
        for (var octave = 0; octave < octaves; ++octave)
        {
            var normOctave = amplitude;
            FieldOctave(field, width, height, (int)wavelength / (1 << octave), seed, octave,
                amplitude);
            norm += normOctave;
            amplitude *= OctaveFalloff;
        }

        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x) field[x, y] /= norm;
        });

        return field;
    }

    /// <summary>
    ///     Accumulates one Worley octave into the field, caching the jittered feature points
    ///     of each grid row.
    /// </summary>
    /// <param name="field">Field accumulator, updated in place.</param>
    /// <param name="width">Field width in blocks.</param>
    /// <param name="height">Field height in blocks.</param>
    /// <param name="cellSize">Grid cell size in blocks.</param>
    /// <param name="seed">Seed for the feature point jitter.</param>
    /// <param name="octave">Octave index, mixed into the jitter.</param>
    /// <param name="amplitude">Amplitude of this octave.</param>
    private static void FieldOctave(float[,] field, int width, int height, int cellSize,
        ulong seed, int octave, float amplitude)
    {
        var gridRows = height / cellSize + 1;
        Parallel.For(0, gridRows, gy =>
        {
            // Feature points of the grid rows gy-1, gy, gy+1: X and Y coordinates. Column
            // indices are offset by one so cx = -1 lives at index 0.
            var cellsAcross = width / cellSize + 1;
            var points = new float[3, cellsAcross + 3, 2];
            for (var ry = 0; ry < 3; ++ry)
            {
                var cy = gy + ry - 1;
                for (var cx = -1; cx <= cellsAcross + 1; ++cx)
                {
                    var state = SeedDerivation.SplitMix64(
                        seed ^ ((ulong)(uint)cx << 32) ^ (uint)cy ^ ((ulong)(uint)octave << 56));
                    var jx = (SeedDerivation.Next(ref state) >> 11) * UniformScale;
                    var jy = (SeedDerivation.Next(ref state) >> 11) * UniformScale;
                    points[ry, cx + 1, 0] = (cx + (float)jx) * cellSize;
                    points[ry, cx + 1, 1] = (cy + (float)jy) * cellSize;
                }
            }

            var yMin = gy * cellSize;
            var yMax = Math.Min(yMin + cellSize, height);
            for (var y = yMin; y < yMax; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var gx = x / cellSize;
                    var f1 = float.MaxValue;
                    var f2 = float.MaxValue;
                    for (var ry = 0; ry < 3; ++ry)
                    {
                        for (var rx = gx - 1; rx <= gx + 1; ++rx)
                        {
                            var ddx = points[ry, rx + 1, 0] - x;
                            var ddy = points[ry, rx + 1, 1] - y;
                            var d = ddx * ddx + ddy * ddy;
                            if (d < f1)
                            {
                                f2 = f1;
                                f1 = d;
                            }
                            else if (d < f2)
                            {
                                f2 = d;
                            }
                        }
                    }

                    field[x, y] += amplitude
                                   * (MathF.Sqrt(f2) - MathF.Sqrt(f1)) / cellSize;
                }
            }
        });
    }
}
