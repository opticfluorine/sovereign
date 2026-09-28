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

namespace Sovereign.WorldGen.Caves;

/// <summary>
///     One column of a spiral staircase shaft, with the floor Z of each step carved in
///     that column.
/// </summary>
public sealed class CaveShaftColumn
{
    /// <summary>
    ///     X coordinate of the column.
    /// </summary>
    public required int X { get; init; }

    /// <summary>
    ///     Y coordinate of the column.
    /// </summary>
    public required int Y { get; init; }

    /// <summary>
    ///     Floor Z of each step in this column, in ascending walk order.
    /// </summary>
    public required IReadOnlyList<int> StepZs { get; init; }
}

/// <summary>
///     A spiral staircase shaft connecting two adjacent cave levels. The shaft carves its
///     3x3 footprint open between the two level floors and re-solidifies the surrounding
///     ring so that a shaft crossing a chamber wall merges cleanly.
/// </summary>
public sealed class CaveShaft
{
    /// <summary>
    ///     Index of the upper level of the pair.
    /// </summary>
    public required int UpperLevel { get; init; }

    /// <summary>
    ///     Index of the lower level of the pair.
    /// </summary>
    public required int LowerLevel { get; init; }

    /// <summary>
    ///     X coordinate of the shaft center column.
    /// </summary>
    public required int CenterX { get; init; }

    /// <summary>
    ///     Y coordinate of the shaft center column.
    /// </summary>
    public required int CenterY { get; init; }

    /// <summary>
    ///     Floor Z at the bottom of the shaft, at the lower level's floor.
    /// </summary>
    public required int LowerFloorZ { get; init; }

    /// <summary>
    ///     Floor Z at the top of the shaft, at the upper level's floor.
    /// </summary>
    public required int UpperFloorZ { get; init; }

    /// <summary>
    ///     Headroom in blocks carved above each step.
    /// </summary>
    public required int Headroom { get; init; }

    /// <summary>
    ///     Stair step columns of the shaft: the eight cells surrounding the core, each with
    ///     the Z of every step it carries.
    /// </summary>
    public required IReadOnlyList<CaveShaftColumn> Columns { get; init; }

    /// <summary>
    ///     Columns of the ring outside the 3x3 footprint, re-solidified at all Z in
    ///     <c>[LowerFloorZ, UpperFloorZ + Headroom]</c>.
    /// </summary>
    public required IReadOnlyList<(int X, int Y)> Ring { get; init; }

    /// <summary>
    ///     Z range top of the re-solidified ring.
    /// </summary>
    public int RingTopZ => UpperFloorZ + Headroom;
}

/// <summary>
///     A surface mouth: a 2x2 open shaft from the surface down to the shallowest cave
///     level, with interior spiral stairs cut into the shaft wall.
/// </summary>
public sealed class CaveMouth
{
    /// <summary>
    ///     X coordinate of the upper-left column of the 2x2 shaft.
    /// </summary>
    public required int X { get; init; }

    /// <summary>
    ///     Y coordinate of the upper-left column of the 2x2 shaft.
    /// </summary>
    public required int Y { get; init; }

    /// <summary>
    ///     Surface height at the mouth.
    /// </summary>
    public required int SurfaceZ { get; init; }

    /// <summary>
    ///     Floor Z at the bottom of the mouth, at the shallowest cave level's floor.
    /// </summary>
    public required int FloorZ { get; init; }

    /// <summary>
    ///     Headroom in blocks carved above each step and the cave floor.
    /// </summary>
    public required int Headroom { get; init; }

    /// <summary>
    ///     Stair step columns of the mouth, each with the Z of every step it carries.
    /// </summary>
    public required IReadOnlyList<CaveShaftColumn> Columns { get; init; }
}

/// <summary>
///     Per-level carved data of one cave level. All arrays are indexed <c>[x, y]</c> over
///     footprint-local coordinates; cells that are not open carry undefined floor data.
/// </summary>
public sealed class CaveLevelMap
{
    /// <summary>
    ///     Width of the map in blocks.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the map in blocks.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Base floor Z of the level from the profile.
    /// </summary>
    public required int BaseFloorZ { get; init; }

    /// <summary>
    ///     Vertical clearance of the level in blocks.
    /// </summary>
    public required int Headroom { get; init; }

    /// <summary>
    ///     Whether each cell is a carved walkable cell at the level's floor.
    /// </summary>
    public required bool[,] Open { get; init; }

    /// <summary>
    ///     Actual floor Z of each open cell: base floor plus the floor field.
    /// </summary>
    public required int[,] FloorZ { get; init; }

    /// <summary>
    ///     Air blocks above the floor of each open cell; headroom may vary near shafts and
    ///     mouths.
    /// </summary>
    public required byte[,] CarveHeight { get; init; }

    /// <summary>
    ///     Quantized Worley F2-F1 field used for the porosity mask; retained for the cave
    ///     preview's corridor-versus-chamber shading.
    /// </summary>
    public required byte[,] Worley { get; init; }

    /// <summary>
    ///     Whether each cell was excluded by the water-proximity rule.
    /// </summary>
    public required bool[,] WaterExcluded { get; init; }
}

/// <summary>
///     Output cave map of a world generation plan. Cave carving is expressed as cave-air:
///     card 5's segment assembler converts cave cells to air blocks; caves never carry
///     block materials of their own except where the stage explicitly re-solidifies.
/// </summary>
public sealed class CaveMap
{
    /// <summary>
    ///     Width of the map in blocks.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the map in blocks.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Per-level carved data, ordered from top to bottom as in the profile.
    /// </summary>
    public required IReadOnlyList<CaveLevelMap> Levels { get; init; }

    /// <summary>
    ///     Staircase shafts between adjacent level pairs.
    /// </summary>
    public required IReadOnlyList<CaveShaft> Shafts { get; init; }

    /// <summary>
    ///     Surface mouths on the shallowest level.
    /// </summary>
    public required IReadOnlyList<CaveMouth> MouthList { get; init; }

    /// <summary>
    ///     Surface mouth columns: the 2x2 shaft columns of every placed mouth.
    /// </summary>
    public IReadOnlyList<(int X, int Y)> Mouths => mouths ??= BuildMouthColumns();

    /// <summary>
    ///     Memoized mouth column list.
    /// </summary>
    private IReadOnlyList<(int X, int Y)>? mouths;

    /// <summary>
    ///     Per level open-cell counts, in level order.
    /// </summary>
    public int[] LevelOpenCounts { get; private set; } = null!;

    /// <summary>
    ///     Counts the open cells per level, filling <see cref="LevelOpenCounts" />. Called
    ///     by the cave stage once carving is final.
    /// </summary>
    public void CountOpenCells()
    {
        var counts = new int[Levels.Count];
        for (var i = 0; i < Levels.Count; ++i)
        {
            var open = Levels[i].Open;
            var count = 0;
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    if (open[x, y]) ++count;
                }
            }

            counts[i] = count;
        }

        LevelOpenCounts = counts;
    }

    /// <summary>
    ///     Builds the flat mouth column list from the placed mouths.
    /// </summary>
    /// <returns>Mouth columns in placement order.</returns>
    private IReadOnlyList<(int X, int Y)> BuildMouthColumns()
    {
        var columns = new List<(int X, int Y)>(MouthList.Count * 4);
        foreach (var mouth in MouthList)
        {
            columns.Add((mouth.X, mouth.Y));
            columns.Add((mouth.X + 1, mouth.Y));
            columns.Add((mouth.X, mouth.Y + 1));
            columns.Add((mouth.X + 1, mouth.Y + 1));
        }

        return columns;
    }
}
