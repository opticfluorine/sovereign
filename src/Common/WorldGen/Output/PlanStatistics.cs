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

using System.Text;

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
    ///     Formats the statistics as a chat-ready block.
    /// </summary>
    /// <returns>Formatted statistics.</returns>
    public string Format()
    {
        var total = LandCells + WaterCells;
        var landPct = total > 0 ? 100.0 * LandCells / total : 0.0;
        var waterPct = total > 0 ? 100.0 * WaterCells / total : 0.0;

        return new StringBuilder()
            .AppendLine($"World generation plan ({Width}x{Height}):")
            .AppendLine($"  Land: {landPct:F1}%  Water: {waterPct:F1}%")
            .AppendLine($"  Rivers: {RiverCount}  Lakes: {LakeCount}")
            .Append(
                $"  Terrain: {FormatSeconds(TerrainMs)}  Hydrology: {FormatSeconds(HydrologyMs)}  " +
                $"Preview: {FormatSeconds(PreviewMs)}  Total: {FormatSeconds(TotalMs)}")
            .ToString();
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
