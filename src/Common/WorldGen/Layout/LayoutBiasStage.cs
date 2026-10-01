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

using System;
using System.Collections.Generic;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     A layout anchor resolved against a plan footprint: center jittered by the layout
///     sub-seed, ready for bump accumulation.
/// </summary>
public sealed class ResolvedAnchor
{
    /// <summary>
    ///     Zero-based anchor index in the resolved order.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    ///     Anchor center X in normalized footprint coordinates.
    /// </summary>
    public required float X { get; init; }

    /// <summary>
    ///     Anchor center Y in normalized footprint coordinates.
    /// </summary>
    public required float Y { get; init; }

    /// <summary>
    ///     Bump radius in normalized footprint coordinates.
    /// </summary>
    public required float Radius { get; init; }

    /// <summary>
    ///     Peak mask contribution of the bump.
    /// </summary>
    public required float Weight { get; init; }

    /// <summary>
    ///     Mountain bias at the anchor.
    /// </summary>
    public required float MountainBias { get; init; }

    /// <summary>
    ///     Temperature bias at the anchor.
    /// </summary>
    public required float TemperatureBias { get; init; }

    /// <summary>
    ///     Moisture bias at the anchor.
    /// </summary>
    public required float MoistureBias { get; init; }

    /// <summary>
    ///     Roughness bias at the anchor.
    /// </summary>
    public required float RoughnessBias { get; init; }
}

/// <summary>
///     Quarter-resolution bias fields accumulated from the resolved anchors. Each field
///     sums <c>weight * bias * smoothstep(...)</c> per anchor and is clamped to [-1, 1].
/// </summary>
public sealed class LayoutBiasFields
{
    /// <summary>
    ///     Mountain bias field.
    /// </summary>
    public required float[,] Mountain { get; init; }

    /// <summary>
    ///     Temperature bias field.
    /// </summary>
    public required float[,] Temperature { get; init; }

    /// <summary>
    ///     Moisture bias field.
    /// </summary>
    public required float[,] Moisture { get; init; }

    /// <summary>
    ///     Roughness bias field.
    /// </summary>
    public required float[,] Roughness { get; init; }

    /// <summary>
    ///     Snowcap climate-line shift field in Z: the mountain bias scaled by
    ///     <see cref="LayoutBiasStage.SnowcapBiasCoefficient" />.
    /// </summary>
    public required float[,] SnowcapShift { get; init; }

    /// <summary>
    ///     Alpine climate-line shift field in Z: the mountain bias scaled by
    ///     <see cref="LayoutBiasStage.AlpineBiasCoefficient" />.
    /// </summary>
    public required float[,] AlpineShift { get; init; }
}

/// <summary>
///     Builds the quarter-resolution bias fields from resolved anchors on the same
///     bump-accumulator infrastructure as the continentalness mask.
/// </summary>
public static class LayoutBiasStage
{
    /// <summary>
    ///     Z that a full positive mountain bias lowers the snowcap line by.
    /// </summary>
    public const float SnowcapBiasCoefficient = 4f;

    /// <summary>
    ///     Z that a full positive mountain bias lowers the alpine line by.
    /// </summary>
    public const float AlpineBiasCoefficient = 3f;

    /// <summary>
    ///     Builds the four bias fields and the two climate-line shift fields over a
    ///     quarter-resolution grid.
    /// </summary>
    /// <param name="anchors">Resolved anchors.</param>
    /// <param name="quarterWidth">Quarter-grid width.</param>
    /// <param name="quarterHeight">Quarter-grid height.</param>
    /// <param name="width">Footprint width in blocks.</param>
    /// <param name="height">Footprint height in blocks.</param>
    /// <returns>Quarter-resolution bias fields.</returns>
    public static LayoutBiasFields Build(IReadOnlyList<ResolvedAnchor> anchors, int quarterWidth,
        int quarterHeight, int width, int height)
    {
        var mountain = new float[quarterWidth, quarterHeight];
        var temperature = new float[quarterWidth, quarterHeight];
        var moisture = new float[quarterWidth, quarterHeight];
        var roughness = new float[quarterWidth, quarterHeight];
        var snowcapShift = new float[quarterWidth, quarterHeight];
        var alpineShift = new float[quarterWidth, quarterHeight];

        for (var qy = 0; qy < quarterHeight; ++qy)
        {
            var v = (4 * qy + 1.5f) / height;
            for (var qx = 0; qx < quarterWidth; ++qx)
            {
                var u = (4 * qx + 1.5f) / width;
                var mountainSum = 0f;
                var temperatureSum = 0f;
                var moistureSum = 0f;
                var roughnessSum = 0f;
                foreach (var anchor in anchors)
                {
                    var bump = LayoutMaskStage.BumpAt(anchor, u, v);
                    if (bump <= 0f) continue;

                    mountainSum += bump * anchor.MountainBias;
                    temperatureSum += bump * anchor.TemperatureBias;
                    moistureSum += bump * anchor.MoistureBias;
                    roughnessSum += bump * anchor.RoughnessBias;
                }

                var mountainBias = Math.Clamp(mountainSum, -1f, 1f);
                mountain[qx, qy] = mountainBias;
                temperature[qx, qy] = Math.Clamp(temperatureSum, -1f, 1f);
                moisture[qx, qy] = Math.Clamp(moistureSum, -1f, 1f);
                roughness[qx, qy] = Math.Clamp(roughnessSum, -1f, 1f);
                snowcapShift[qx, qy] = mountainBias * SnowcapBiasCoefficient;
                alpineShift[qx, qy] = mountainBias * AlpineBiasCoefficient;
            }
        }

        return new LayoutBiasFields
        {
            Mountain = mountain,
            Temperature = temperature,
            Moisture = moisture,
            Roughness = roughness,
            SnowcapShift = snowcapShift,
            AlpineShift = alpineShift
        };
    }
}
