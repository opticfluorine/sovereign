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
///     Biome classification of a single cell. Water, shelf, beach, river, and lake IDs are
///     assigned from the terrain map flags; the noise-driven Whittaker table only classifies
///     land cells.
/// </summary>
public enum BiomeId
{
    /// <summary>
    ///     Deep ocean beyond the shelf break.
    /// </summary>
    Ocean = 0,

    /// <summary>
    ///     Submerged seafloor between the shelf break and the coast.
    /// </summary>
    Shelf = 1,

    /// <summary>
    ///     Land cell in the beach band adjacent to water.
    /// </summary>
    Beach = 2,

    /// <summary>
    ///     Land cell covered by a lake.
    /// </summary>
    Lake = 3,

    /// <summary>
    ///     Cell on an extracted river.
    /// </summary>
    River = 4,

    /// <summary>
    ///     Temperate grassland.
    /// </summary>
    Grassland = 5,

    /// <summary>
    ///     Temperate forest.
    /// </summary>
    Forest = 6,

    /// <summary>
    ///     Cold coniferous forest.
    /// </summary>
    Taiga = 7,

    /// <summary>
    ///     Hot desert.
    /// </summary>
    Desert = 8,

    /// <summary>
    ///     Hot tropical savanna.
    /// </summary>
    Savanna = 9,

    /// <summary>
    ///     Waterlogged lowland ground-truth override biome.
    /// </summary>
    Swamp = 10,

    /// <summary>
    ///     Rocky alpine belt below the snow line.
    /// </summary>
    Alpine = 11,

    /// <summary>
    ///     Snow-covered peaks at or above the snow line.
    /// </summary>
    Snowcap = 12
}
