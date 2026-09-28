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

using System.Threading.Tasks;

namespace Sovereign.WorldGen.Terrain;

/// <summary>
///     Sea-level band class of a single cell.
/// </summary>
public enum ContinentalClass
{
    /// <summary>
    ///     Deep ocean: continentalness below the ocean threshold.
    /// </summary>
    DeepOcean = 0,

    /// <summary>
    ///     Shelf: submerged seafloor between the ocean and coast thresholds.
    /// </summary>
    Shelf = 1,

    /// <summary>
    ///     Land above the coast threshold.
    /// </summary>
    Land = 2
}

/// <summary>
///     Banded classification of the continentalness field.
/// </summary>
public sealed class ContinentalnessResult
{
    /// <summary>
    ///     Band class of each cell.
    /// </summary>
    public required ContinentalClass[,] Classes { get; init; }

    /// <summary>
    ///     Normalized continentalness threshold below which cells are deep ocean.
    /// </summary>
    public required float ThresholdOcean { get; init; }

    /// <summary>
    ///     Normalized continentalness threshold below which cells are shelf.
    /// </summary>
    public required float ThresholdCoast { get; init; }

    /// <summary>
    ///     Normalized continentalness threshold above which cells are inland.
    /// </summary>
    public required float ThresholdInland { get; init; }
}

/// <summary>
///     Bands the normalized continentalness field into deep ocean, shelf, and land,
///     marking ocean and shelf cells for the rest of the pipeline.
/// </summary>
public sealed class ContinentalnessStage
{
    /// <summary>
    ///     Bands the continentalness field.
    /// </summary>
    /// <param name="fields">Sampled terrain fields.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <param name="terrain">Terrain generation options.</param>
    /// <returns>Banded classification.</returns>
    public ContinentalnessResult Apply(TerrainFields fields, int width, int height,
        TerrainOptions terrain)
    {
        var classes = new ContinentalClass[width, height];
        var thresholdOcean = terrain.Thresholds.Ocean;
        var thresholdCoast = terrain.Thresholds.Coast;

        System.Threading.Tasks.Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; ++x)
            {
                var value = fields.Continentalness[x, y];
                classes[x, y] = value < thresholdOcean ? ContinentalClass.DeepOcean
                    : value < thresholdCoast ? ContinentalClass.Shelf
                    : ContinentalClass.Land;
            }
        });

        return new ContinentalnessResult
        {
            Classes = classes,
            ThresholdOcean = thresholdOcean,
            ThresholdCoast = thresholdCoast,
            ThresholdInland = terrain.Thresholds.Inland
        };
    }
}
