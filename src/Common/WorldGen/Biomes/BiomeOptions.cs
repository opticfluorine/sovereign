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

namespace Sovereign.WorldGen;

/// <summary>
///     Biome classification, material, and decoration configuration within a world
///     generation profile. All template names are stored unresolved; strict resolution
///     against live templates happens at commit time.
/// </summary>
public sealed class BiomeOptions
{
    /// <summary>
    ///     Cells with surface height at or above this Z become Snowcap.
    /// </summary>
    public required int SnowcapZ { get; set; }

    /// <summary>
    ///     Cells with surface height in <c>[alpineZ, snowcapZ)</c> become Alpine.
    /// </summary>
    public required int AlpineZ { get; set; }

    /// <summary>
    ///     Surface template for ocean and shelf floors away from the coast.
    /// </summary>
    public required string OceanFloorTemplate { get; set; }

    /// <summary>
    ///     Surface template under fresh water: lake and river cells.
    /// </summary>
    public required string WaterFloorTemplate { get; set; }

    /// <summary>
    ///     3x3 Whittaker table from temperature and moisture bands to biome names.
    /// </summary>
    public required BiomeTableOptions Table { get; set; }

    /// <summary>
    ///     Ground-truth swamp override. Optional; null if absent from the profile.
    /// </summary>
    public SwampOptions? Swamp { get; set; }

    /// <summary>
    ///     Material and decoration definitions by biome name.
    /// </summary>
    public required Dictionary<string, BiomeDefinition> Definitions { get; set; }
}

/// <summary>
///     One row of the Whittaker biome table, mapping moisture bands to biome names.
/// </summary>
public sealed class BiomeTableRow
{
    /// <summary>
    ///     Biome name for the dry moisture band.
    /// </summary>
    public required string Dry { get; set; }

    /// <summary>
    ///     Biome name for the temperate moisture band.
    /// </summary>
    public required string Temperate { get; set; }

    /// <summary>
    ///     Biome name for the wet moisture band.
    /// </summary>
    public required string Wet { get; set; }
}

/// <summary>
///     The 3x3 Whittaker biome table, mapping temperature bands to moisture-band rows.
/// </summary>
public sealed class BiomeTableOptions
{
    /// <summary>
    ///     Cold temperature band row.
    /// </summary>
    public required BiomeTableRow Cold { get; set; }

    /// <summary>
    ///     Mild temperature band row.
    /// </summary>
    public required BiomeTableRow Mild { get; set; }

    /// <summary>
    ///     Hot temperature band row.
    /// </summary>
    public required BiomeTableRow Hot { get; set; }
}

/// <summary>
///     Ground-truth swamp override configuration.
/// </summary>
public sealed class SwampOptions
{
    /// <summary>
    ///     Surface template for swamp cells.
    /// </summary>
    public required string Template { get; set; }

    /// <summary>
    ///     Swamps only occur at or below this surface Z.
    /// </summary>
    public required int MaxHeightZ { get; set; }
}

/// <summary>
///     Surface material and decoration configuration for a single biome.
/// </summary>
public sealed class BiomeDefinition
{
    /// <summary>
    ///     Template name of the top block of the biome's columns.
    /// </summary>
    public required string SurfaceTemplate { get; set; }

    /// <summary>
    ///     Template name of the block below the surface of the biome's columns.
    /// </summary>
    public required string SubSurfaceTemplate { get; set; }

    /// <summary>
    ///     Number of blocks of subsurface material, in <c>[0, 16]</c>.
    /// </summary>
    public required int SubSurfaceDepth { get; set; }

    /// <summary>
    ///     Decoration pool for the biome, in declared order. Optional; null if absent,
    ///     which yields a silent biome and a validation warning when the table references it.
    /// </summary>
    public List<DecorationOptions>? Decorations { get; set; }
}

/// <summary>
///     A single decoration pool entry.
/// </summary>
public sealed class DecorationOptions
{
    /// <summary>
    ///     Name of the decoration template to place.
    /// </summary>
    public required string Template { get; set; }

    /// <summary>
    ///     Placement probability per column draw, in <c>(0, 1]</c>.
    /// </summary>
    public required double Weight { get; set; }

    /// <summary>
    ///     Minimum Chebyshev distance between two placements of this template.
    /// </summary>
    public required int MinSpacing { get; set; }

    /// <summary>
    ///     Maximum surface slope, in blocks to the 4-neighbors, on which this decoration
    ///     may be placed.
    /// </summary>
    public required int MaxSlope { get; set; }
}
