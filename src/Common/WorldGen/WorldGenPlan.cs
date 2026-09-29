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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen;

/// <summary>
///     A completed world generation plan.
/// </summary>
public sealed class WorldGenPlan
{
    /// <summary>
    ///     Root world seed used to generate the plan.
    /// </summary>
    public required ulong Seed { get; init; }

    /// <summary>
    ///     Name of the world generation profile used to generate the plan.
    /// </summary>
    public required string ProfileName { get; init; }

    /// <summary>
    ///     Validated profile used to generate the plan.
    /// </summary>
    public required WorldGenProfile Profile { get; init; }

    /// <summary>
    ///     World X coordinate of footprint-local cell (0, 0).
    /// </summary>
    public required int OriginX { get; init; }

    /// <summary>
    ///     World Y coordinate of footprint-local cell (0, 0).
    /// </summary>
    public required int OriginY { get; init; }

    /// <summary>
    ///     Profile template names resolved against the live template entity set.
    /// </summary>
    public required WorldGenResolvedTemplates ResolvedTemplates { get; init; }

    /// <summary>
    ///     Absolute path of the staged plan directory holding the assembled segment
    ///     blobs, staged decorations, and manifest. Empty if the plan has not been staged.
    /// </summary>
    public string StagingDirectory { get; init; } = "";

    /// <summary>
    ///     Terrain surface map of the plan.
    /// </summary>
    public required TerrainMap Terrain { get; init; }

    /// <summary>
    ///     Biome classification map of the plan. Null if the profile has no biomes section.
    /// </summary>
    public BiomeMap? Biomes { get; init; }

    /// <summary>
    ///     Per-column material assignment of the plan. Null if the profile has no biomes
    ///     section.
    /// </summary>
    public ColumnMaterials? Materials { get; init; }

    /// <summary>
    ///     Decoration placements of the plan, in scan order. Null if the profile has no
    ///     biomes section.
    /// </summary>
    public IReadOnlyList<DecorationPlacement>? Decorations { get; init; }

    /// <summary>
    ///     Carved cave map of the plan. Null if the profile has no cave levels.
    /// </summary>
    public CaveMap? Caves { get; init; }

    /// <summary>
    ///     Absolute paths of the rendered cave preview images, one per cave level in
    ///     top-to-bottom order. Empty if the profile has no cave levels.
    /// </summary>
    public IReadOnlyList<string> CavePreviewPaths { get; init; } = new List<string>();

    /// <summary>
    ///     Summary statistics of the plan.
    /// </summary>
    public required Output.PlanStatistics Statistics { get; init; }

    /// <summary>
    ///     Absolute path of the rendered preview image.
    /// </summary>
    public required string PreviewPath { get; init; }
}
