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

namespace Sovereign.WorldGen;

/// <summary>
///     Cave generation options within a world generation profile. Every option keeps its
///     shipped default when absent from the JSON, so partial sections are valid.
/// </summary>
public sealed class CaveOptions
{
    /// <summary>
    ///     Default fraction of a cave level's area that is open.
    /// </summary>
    public const double DefaultPorosity = 0.34;

    /// <summary>
    ///     Default corridor carve width in blocks.
    /// </summary>
    public const int DefaultMinTunnelWidth = 2;

    /// <summary>
    ///     Default surface mouth exclusion radius for water, in blocks.
    /// </summary>
    public const int DefaultMouthMinLandDistance = 16;

    /// <summary>
    ///     Number of vertical shafts to generate between each adjacent pair of cave levels.
    /// </summary>
    public int ShaftsPerLevelPair { get; set; }

    /// <summary>
    ///     Number of surface cave mouths to generate.
    /// </summary>
    public int SurfaceMouths { get; set; }

    /// <summary>
    ///     Fraction of a cave level's area that is open, in [0.1, 0.6].
    /// </summary>
    public double Porosity { get; set; } = DefaultPorosity;

    /// <summary>
    ///     Corridor carve width in blocks, in [1, 4].
    /// </summary>
    public int MinTunnelWidth { get; set; } = DefaultMinTunnelWidth;

    /// <summary>
    ///     Mouth exclusion radius for water in blocks, in [4, 256]: no surface mouth may
    ///     place within this distance of a water, bank, or river cell.
    /// </summary>
    public int MouthMinLandDistance { get; set; } = DefaultMouthMinLandDistance;
}
