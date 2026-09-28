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

namespace Sovereign.WorldGen.Biomes;

/// <summary>
///     Per-column block material assignment for a plan footprint. Template names are stored
///     unresolved; strict resolution against live templates happens at commit time. All
///     arrays are indexed <c>[x, y]</c> over footprint-local coordinates.
/// </summary>
public sealed class ColumnMaterials
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
    ///     Template name of the top block of each column.
    /// </summary>
    public required string[,] SurfaceTemplate { get; init; }

    /// <summary>
    ///     Template name of the block below the surface of each column.
    /// </summary>
    public required string[,] SubSurfaceTemplate { get; init; }

    /// <summary>
    ///     Number of blocks of subsurface material in each column; the stone bands begin
    ///     below the subsurface band.
    /// </summary>
    public required int[,] SubSurfaceDepth { get; init; }

    /// <summary>
    ///     Seeded per-column material modifier in [0, 3].
    /// </summary>
    public required byte[,] SurfaceModifier { get; init; }
}
