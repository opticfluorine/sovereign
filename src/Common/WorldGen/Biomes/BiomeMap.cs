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
///     Biome classification of every cell in a plan footprint. All arrays are indexed
///     <c>[x, y]</c> over footprint-local coordinates in the range <c>0..width-1</c>,
///     <c>0..height-1</c>.
/// </summary>
public sealed class BiomeMap
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
    ///     Biome ID of each cell; every cell is classified with exactly one ID.
    /// </summary>
    public required BiomeId[,] Biome { get; init; }
}
