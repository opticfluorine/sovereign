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

namespace Sovereign.WorldGen.Terrain;

/// <summary>
///     Output surface map of a world generation plan. All arrays are indexed
///     <c>[x, y]</c> over footprint-local coordinates in the range
///     <c>0..width-1</c>, <c>0..height-1</c>; origin offsets are applied only
///     when producing world coordinates.
/// </summary>
public sealed class TerrainMap
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
    ///     Surface height of each cell in block Z coordinates.
    /// </summary>
    public required int[,] Heights { get; init; }

    /// <summary>
    ///     Whether each cell is water: deep ocean or shelf.
    /// </summary>
    public required bool[,] IsOcean { get; init; }

    /// <summary>
    ///     Whether each cell was tagged as a cliff after slope relaxation.
    /// </summary>
    public required bool[,] IsCliff { get; init; }

    /// <summary>
    ///     Whether each cell is a land cell in the beach band adjacent to water.
    /// </summary>
    public required bool[,] IsBeach { get; init; }

    /// <summary>
    ///     Whether each cell lies on an extracted river.
    /// </summary>
    public required bool[,] IsRiver { get; init; }

    /// <summary>
    ///     River channel width in blocks; zero away from rivers.
    /// </summary>
    public required int[,] RiverWidth { get; init; }

    /// <summary>
    ///     Whether each cell is a non-river bank adjacent to a river.
    /// </summary>
    public required bool[,] IsBank { get; init; }

    /// <summary>
    ///     Whether each cell is covered by a lake.
    /// </summary>
    public required bool[,] IsLake { get; init; }

    /// <summary>
    ///     Water plane Z of each lake cell; zero away from lakes.
    /// </summary>
    public required int[,] LakeSurfaceZ { get; init; }
}
