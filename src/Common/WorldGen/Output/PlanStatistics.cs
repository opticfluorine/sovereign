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

using System.Collections.Generic;
using System.Text;
using Sovereign.WorldGen.Biomes;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Summary statistics of a completed world generation plan.
/// </summary>
public sealed class PlanStatistics
{
    /// <summary>
    ///     Width of the plan footprint in blocks.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the plan footprint in blocks.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Number of land cells.
    /// </summary>
    public required int LandCells { get; init; }

    /// <summary>
    ///     Number of water cells: deep ocean and shelf.
    /// </summary>
    public required int WaterCells { get; init; }

    /// <summary>
    ///     Number of extracted rivers.
    /// </summary>
    public required int RiverCount { get; init; }

    /// <summary>
    ///     Number of extracted lakes.
    /// </summary>
    public required int LakeCount { get; init; }

    /// <summary>
    ///     Longest maximal run of constant step direction over the extracted rivers, in
    ///     cells, measured after straight-run trimming.
    /// </summary>
    public required int LongestStraightRiverRun { get; init; }

    /// <summary>
    ///     Terrain stages wall time in milliseconds.
    /// </summary>
    public required long TerrainMs { get; init; }

    /// <summary>
    ///     Hydrology stages wall time in milliseconds.
    /// </summary>
    public required long HydrologyMs { get; init; }

    /// <summary>
    ///     Preview rendering wall time in milliseconds.
    /// </summary>
    public required long PreviewMs { get; init; }

    /// <summary>
    ///     Total plan wall time in milliseconds.
    /// </summary>
    public required long TotalMs { get; init; }

    /// <summary>
    ///     Biome stages wall time in milliseconds.
    /// </summary>
    public required long BiomesMs { get; init; }

    /// <summary>
    ///     Cave stage wall time in milliseconds; zero when the profile has no cave levels.
    /// </summary>
    public long CavesMs { get; init; }

    /// <summary>
    ///     Statistics of the generated cave system. Null if the profile has no cave levels.
    /// </summary>
    public CaveStats? Caves { get; init; }

    /// <summary>
    ///     Fraction of the footprint classified as each biome, ordered by biome ID. Null if
    ///     the profile has no biomes section.
    /// </summary>
    public IReadOnlyDictionary<BiomeId, double>? BiomePercentages { get; init; }

    /// <summary>
    ///     Number of placed decorations per template name, ordered by name. Null if the
    ///     profile has no biomes section.
    /// </summary>
    public IReadOnlyDictionary<string, int>? DecorationCounts { get; init; }

    /// <summary>
    ///     Total number of placed decorations; zero if the profile has no biomes section.
    /// </summary>
    public int DecorationsTotal { get; init; }

    /// <summary>
    ///     Formats the statistics as a chat-ready block.
    /// </summary>
    /// <returns>Formatted statistics.</returns>
    public string Format()
    {
        var total = LandCells + WaterCells;
        var landPct = total > 0 ? 100.0 * LandCells / total : 0.0;
        var waterPct = total > 0 ? 100.0 * WaterCells / total : 0.0;

        var builder = new StringBuilder()
            .AppendLine($"World generation plan ({Width}x{Height}):")
            .AppendLine($"  Land: {landPct:F1}%  Water: {waterPct:F1}%")
            .AppendLine(
                $"  Rivers: {RiverCount}  Lakes: {LakeCount}  " +
                $"LongestStraightRiverRun: {LongestStraightRiverRun}");
        AppendBiomeLines(builder);
        AppendDecorationLines(builder);
        AppendCaveLines(builder);
        builder.Append(
            $"  Terrain: {FormatSeconds(TerrainMs)}  Hydrology: {FormatSeconds(HydrologyMs)}  " +
            $"Biomes: {FormatSeconds(BiomesMs)}  Caves: {FormatSeconds(CavesMs)}  " +
            $"Preview: {FormatSeconds(PreviewMs)}  Total: {FormatSeconds(TotalMs)}");
        return builder.ToString();
    }

    /// <summary>
    ///     Appends the cave statistics block, or nothing when the plan carries no cave data.
    /// </summary>
    /// <param name="builder">Builder to append to.</param>
    private void AppendCaveLines(StringBuilder builder)
    {
        if (Caves is null) return;

        builder.AppendLine(Caves.Format());
    }

    /// <summary>
    ///     Appends the biome percentage lines, or a not-configured line when the plan carries
    ///     no biome data.
    /// </summary>
    /// <param name="builder">Builder to append to.</param>
    private void AppendBiomeLines(StringBuilder builder)
    {
        if (BiomePercentages is null)
        {
            builder.AppendLine("  Biomes: not configured");
            return;
        }

        foreach (var (biome, fraction) in BiomePercentages)
        {
            builder.AppendLine($"  Biome {biome}: {fraction:F4}");
        }
    }

    /// <summary>
    ///     Appends the decoration count line, or a not-configured line when the plan carries
    ///     no biome data.
    /// </summary>
    /// <param name="builder">Builder to append to.</param>
    private void AppendDecorationLines(StringBuilder builder)
    {
        if (DecorationCounts is null)
        {
            builder.AppendLine("  Decorations: not configured");
            return;
        }

        var counts = new StringBuilder();
        foreach (var (template, count) in DecorationCounts)
        {
            if (counts.Length > 0) counts.Append(", ");
            counts.Append($"{template} {count}");
        }

        builder.AppendLine(counts.Length > 0
            ? $"  Decorations: {DecorationsTotal} total: {counts}"
            : $"  Decorations: {DecorationsTotal} total");
    }

    /// <summary>
    ///     Formats a duration in milliseconds as seconds.
    /// </summary>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    /// <returns>Formatted duration.</returns>
    private static string FormatSeconds(long milliseconds)
    {
        return $"{milliseconds / 1000.0:F1} s";
    }
}
