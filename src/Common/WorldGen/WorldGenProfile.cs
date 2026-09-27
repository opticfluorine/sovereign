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
///     Profile describing the generation parameters of a new world.
/// </summary>
public sealed class WorldGenProfile
{
    /// <summary>
    ///     Width of the world in blocks. Must be a positive multiple of 32 and at most 16384.
    /// </summary>
    public required int Width { get; set; }

    /// <summary>
    ///     Height of the world in blocks. Must be a positive multiple of 32 and at most 16384.
    /// </summary>
    public required int Height { get; set; }

    /// <summary>
    ///     Z level of the sea surface.
    /// </summary>
    public required int SeaLevelZ { get; set; }

    /// <summary>
    ///     Maximum Z level reachable by the terrain surface.
    /// </summary>
    public required int SurfaceMaxZ { get; set; }

    /// <summary>
    ///     Z level of the bottom of the stone bands, directly above the bedrock layer.
    /// </summary>
    public required int RockFloorZ { get; set; }

    /// <summary>
    ///     Z level of the top of the unmodifiable bedrock layer.
    /// </summary>
    public required int BedrockZ { get; set; }

    /// <summary>
    ///     Stone bands filling the underground, ordered from top to bottom.
    /// </summary>
    public required List<StoneBand> StoneBands { get; set; }

    /// <summary>
    ///     Cave levels, ordered from top to bottom. Optional; null if absent from the profile.
    /// </summary>
    public List<CaveLevel>? CaveLevels { get; set; }

    /// <summary>
    ///     River generation options. Optional; null if absent from the profile.
    /// </summary>
    public RiverOptions? Rivers { get; set; }

    /// <summary>
    ///     Cave generation options. Optional; null if absent from the profile.
    /// </summary>
    public CaveOptions? Caves { get; set; }
}
