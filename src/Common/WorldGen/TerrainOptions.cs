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
///     Terrain generation tuning options within a world generation profile. All keys are
///     optional; absent keys keep the shipped defaults.
/// </summary>
public sealed class TerrainOptions
{
    /// <summary>
    ///     Base continentalness wavelength as a fraction of the larger footprint dimension.
    /// </summary>
    public float ContinentalnessWavelengthFactor { get; set; } = 1.75f;

    /// <summary>
    ///     Number of octaves in the continentalness fBm.
    /// </summary>
    public int ContinentalnessOctaves { get; set; } = 6;

    /// <summary>
    ///     Magnitude of the inner domain warp in noise-space.
    /// </summary>
    public float WarpAmplitudeInner { get; set; } = 0.35f;

    /// <summary>
    ///     Magnitude of the outer domain warp in noise-space.
    /// </summary>
    public float WarpAmplitudeOuter { get; set; } = 0.40f;

    /// <summary>
    ///     Continentalness band thresholds, applied to the normalized and contrast-stretched
    ///     continentalness field.
    /// </summary>
    public ContinentalnessThresholdOptions Thresholds { get; set; } = new();

    /// <summary>
    ///     Longest allowed straight run of a river in cells; longer runs are trimmed.
    /// </summary>
    public int MaxStraightRiverRun { get; set; } = 96;
}

/// <summary>
///     Normalized continentalness band thresholds within the terrain options.
/// </summary>
public sealed class ContinentalnessThresholdOptions
{
    /// <summary>
    ///     Threshold below which cells are deep ocean.
    /// </summary>
    public float Ocean { get; set; } = 0.32f;

    /// <summary>
    ///     Threshold below which cells are shelf.
    /// </summary>
    public float Coast { get; set; } = 0.40f;

    /// <summary>
    ///     Threshold above which cells are inland. Carried for later use by the biome and
    ///     mountain stages; unused by banding.
    /// </summary>
    public float Inland { get; set; } = 0.52f;
}
