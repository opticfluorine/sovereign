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

namespace Sovereign.WorldGen.Decorations;

/// <summary>
///     A single decoration placement in a plan footprint. Coordinates are footprint-local;
///     the world offset is applied when the plan is written.
/// </summary>
public sealed class DecorationPlacement
{
    /// <summary>
    ///     Name of the decoration template to place.
    /// </summary>
    public required string TemplateName { get; init; }

    /// <summary>
    ///     Footprint-local X coordinate of the placement.
    /// </summary>
    public required int X { get; init; }

    /// <summary>
    ///     Footprint-local Y coordinate of the placement.
    /// </summary>
    public required int Y { get; init; }

    /// <summary>
    ///     Footprint-local Z of the placement: the surface top at the time of placement.
    /// </summary>
    public required int Z { get; init; }

    /// <summary>
    ///     Biome the placement belongs to.
    /// </summary>
    public required Biomes.BiomeId Biome { get; init; }

    /// <summary>
    ///     Index of the placement within its biome's decoration pool.
    /// </summary>
    public required int PoolIndex { get; init; }
}
