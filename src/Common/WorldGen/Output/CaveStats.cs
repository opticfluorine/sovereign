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

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Per-level statistics of a generated cave system.
/// </summary>
public sealed class CaveLevelStats
{
    /// <summary>
    ///     Level number, one-based, in top-to-bottom profile order.
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    ///     Base floor Z of the level from the profile.
    /// </summary>
    public required int BaseFloorZ { get; init; }

    /// <summary>
    ///     Number of open cells after exclusion and repair.
    /// </summary>
    public required int OpenCells { get; init; }

    /// <summary>
    ///     Fraction of the level area that is open.
    /// </summary>
    public required double OpenFraction { get; init; }

    /// <summary>
    ///     Configured porosity of the cave system.
    /// </summary>
    public required double ConfiguredPorosity { get; init; }

    /// <summary>
    ///     Cells carved by repair corridors.
    /// </summary>
    public required int RepairCorridorCells { get; init; }

    /// <summary>
    ///     Number of open-cell components before connectivity repair.
    /// </summary>
    public required int ComponentsBeforeRepair { get; init; }

    /// <summary>
    ///     Number of open-cell components after connectivity repair.
    /// </summary>
    public required int ComponentsAfterRepair { get; init; }

    /// <summary>
    ///     Number of shafts attached to the level.
    /// </summary>
    public required int ShaftCount { get; init; }

    /// <summary>
    ///     Number of surface mouths on the level; zero below the shallowest level.
    /// </summary>
    public required int MouthCount { get; init; }
}

/// <summary>
///     Statistics of a generated cave system, with tuning warnings and the
///     water-proximity audit.
/// </summary>
public sealed class CaveStats
{
    /// <summary>
    ///     Per-level statistics in top-to-bottom profile order.
    /// </summary>
    public required IReadOnlyList<CaveLevelStats> Levels { get; init; }

    /// <summary>
    ///     Number of open cave cells that violate the water-proximity rule; nonzero
    ///     indicates a generation bug.
    /// </summary>
    public required int WaterProximityViolations { get; init; }

    /// <summary>
    ///     Tuning and resample warnings emitted during generation.
    /// </summary>
    public required IReadOnlyList<string> Warnings { get; init; }

    /// <summary>
    ///     Formats the cave statistics as a chat-ready block, one line per level.
    /// </summary>
    /// <returns>Formatted statistics.</returns>
    public string Format()
    {
        var builder = new StringBuilder();
        foreach (var level in Levels)
        {
            builder.AppendLine(
                $"  Cave level {level.Level} (floor {level.BaseFloorZ}): " +
                $"{100.0 * level.OpenFraction:F1}% open ({level.OpenCells} cells), " +
                $"repair {level.RepairCorridorCells}, " +
                $"components {level.ComponentsBeforeRepair}->{level.ComponentsAfterRepair}, " +
                $"shafts {level.ShaftCount}, mouths {level.MouthCount}");
        }

        builder.AppendLine(
            $"  Cave water-proximity audit: {WaterProximityViolations} violations");
        foreach (var warning in Warnings)
        {
            builder.AppendLine($"  Cave warning: {warning}");
        }

        return builder.ToString().TrimEnd('\n');
    }
}
