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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Rendered preview image.
/// </summary>
public sealed class PreviewImage
{
    /// <summary>
    ///     Width of the image in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the image in pixels.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Packed RGB pixel data in row-major order, top row first.
    /// </summary>
    public required byte[] Pixels { get; init; }
}

/// <summary>
///     Renders a hillshaded preview of a terrain map, one pixel per block. With a biome map
///     the preview uses fixed per-biome colors; without one it falls back to water, beach,
///     cliff, and height-class coloring. Downscaled to at most 1024 pixels on the long side.
///     The preview is a diagnostic image, not art.
/// </summary>
public sealed class PreviewRenderer
{
    /// <summary>
    ///     Hillshade sensitivity to height differences.
    /// </summary>
    private const float HillshadeGain = 0.15f;

    /// <summary>
    ///     Lower clamp of the hillshade multiplier.
    /// </summary>
    private const float HillshadeMin = 0.6f;

    /// <summary>
    ///     Upper clamp of the hillshade multiplier.
    /// </summary>
    private const float HillshadeMax = 1.4f;

    /// <summary>
    ///     Multiplier applied to cliff cells.
    /// </summary>
    private const float CliffTint = 0.7f;

    /// <summary>
    ///     Maximum preview dimension in pixels supported by the renderer.
    /// </summary>
    private const int MaxPreviewDimension = PreviewOptions.MaxMaxDimension;

    /// <summary>
    ///    Surface cave mouth dot color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) MouthDotColor = (220, 40, 40);

    /// <summary>
    ///     Deep ocean color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) DeepOceanColor = (13, 28, 58);

    /// <summary>
    ///     Shelf color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) ShelfColor = (24, 52, 92);

    /// <summary>
    ///     River and lake color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) RiverColor = (52, 96, 168);

    /// <summary>
    ///     Lowland color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) LowlandColor = (86, 138, 72);

    /// <summary>
    ///     Highland color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) HighlandColor = (148, 130, 82);

    /// <summary>
    ///     Mountain color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) MountainColor = (172, 172, 176);
    /// <summary>
    ///     Beach color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) BeachColor = (196, 178, 128);

    /// <summary>
    ///     Fixed biome palette, indexed by <see cref="BiomeId" />. Distinct hues per biome;
    ///     ocean and shelf encode depth. Load-bearing for the biome preview golden hash:
    ///     palette edits are golden-hash edits.
    /// </summary>
    private static readonly (byte R, byte G, byte B)[] BiomeColors =
    {
        (13, 28, 58),      // Ocean
        (24, 52, 92),      // Shelf
        (196, 178, 128),   // Beach
        (58, 112, 180),    // Lake
        (52, 96, 168),     // River
        (110, 158, 68),    // Grassland
        (52, 108, 46),     // Forest
        (64, 104, 88),     // Taiga
        (214, 186, 118),   // Desert
        (178, 166, 92),    // Savanna
        (88, 102, 60),     // Swamp
        (142, 134, 120),   // Alpine
        (234, 238, 242)    // Snowcap
    };

    /// <summary>
    ///     Renders the preview image with the default maximum dimension.
    /// </summary>
    /// <param name="map">Terrain map to render.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Rendered preview image.</returns>
    public PreviewImage Render(TerrainMap map, ContinentalnessResult continentalness,
        WorldGenProfile profile)
    {
        return Render(map, continentalness, profile, PreviewOptions.DefaultMaxDimension);
    }

    /// <summary>
    ///     Renders the preview image, coloring cells by biome when a biome map is given.
    /// </summary>
    /// <param name="map">Terrain map to render.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="maxDimension">Longest allowed side of the rendered image in pixels;
    /// longer footprints are box-downscaled to fit.</param>
    /// <param name="biomes">Classified biome map, or null for height-class coloring.</param>
    /// <param name="mouthDots">Surface cave mouth columns to mark with red dots, or null.
    ///     Dots are applied only after the surface image is complete and never alter any
    ///     other pixel.</param>
    /// <returns>Rendered preview image.</returns>
    public PreviewImage Render(TerrainMap map, ContinentalnessResult continentalness,
        WorldGenProfile profile, int maxDimension, BiomeMap? biomes = null,
        IReadOnlyList<(int X, int Y)>? mouthDots = null)
    {
        var cap = Math.Clamp(maxDimension, PreviewOptions.MinMaxDimension, MaxPreviewDimension);
        var factor = DownscaleFactor(map, cap);
        var outWidth = (map.Width + factor - 1) / factor;
        var outHeight = (map.Height + factor - 1) / factor;
        var pixels = RenderFull(map, continentalness, profile, biomes);

        if (factor > 1) pixels = Downscale(pixels, map.Width, map.Height, outWidth, outHeight, factor);
        if (mouthDots is { Count: > 0 }) DrawMouthDots(pixels, outWidth, outHeight, factor, mouthDots);

        return new PreviewImage { Width = outWidth, Height = outHeight, Pixels = pixels };
    }

    /// <summary>
    ///     Draws small red dots at the downscaled positions of the surface mouth columns.
    /// </summary>
    /// <param name="pixels">Rendered pixels, updated in place.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="factor">Box downscale factor applied to the image.</param>
    /// <param name="mouthDots">Surface mouth columns.</param>
    private static void DrawMouthDots(byte[] pixels, int width, int height, int factor,
        IReadOnlyList<(int X, int Y)> mouthDots)
    {
        foreach (var (mx, my) in mouthDots)
        {
            var px = Math.Min(mx / factor, width - 1);
            var py = Math.Min(my / factor, height - 1);
            var offset = (py * width + px) * 3;
            pixels[offset] = MouthDotColor.R;
            pixels[offset + 1] = MouthDotColor.G;
            pixels[offset + 2] = MouthDotColor.B;
        }
    }

    /// <summary>
    ///     Computes the box-average downscale factor for a terrain map.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="maxDimension">Longest allowed side of the rendered image in pixels.</param>
    /// <returns>Factor such that the long side is at most the maximum preview dimension.</returns>
    private static int DownscaleFactor(TerrainMap map, int maxDimension)
    {
        var longSide = Math.Max(map.Width, map.Height);
        return longSide <= maxDimension ? 1 : (longSide + maxDimension - 1) / maxDimension;
    }

    /// <summary>
    ///     Renders the terrain map at one pixel per block.
    /// </summary>
    /// <param name="map">Terrain map to render.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="biomes">Classified biome map, or null for height-class coloring.</param>
    /// <returns>Packed RGB pixel data in row-major order, top row first.</returns>
    private static byte[] RenderFull(TerrainMap map, ContinentalnessResult continentalness,
        WorldGenProfile profile, BiomeMap? biomes)
    {
        var pixels = new byte[map.Width * map.Height * 3];
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                var color = ColorOf(map, continentalness, profile, biomes, x, y);
                var offset = (y * map.Width + x) * 3;
                pixels[offset] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.B;
            }
        }

        return pixels;
    }

    /// <summary>
    ///     Determines the color of a cell with hillshade and cliff tinting applied.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="biomes">Classified biome map, or null for height-class coloring.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Cell color.</returns>
    private static (byte R, byte G, byte B) ColorOf(TerrainMap map,
        ContinentalnessResult continentalness, WorldGenProfile profile, BiomeMap? biomes,
        int x, int y)
    {
        var baseColor = BaseColorOf(map, continentalness, profile, biomes, x, y);
        var shade = HillshadeOf(map, x, y);
        if (map.IsCliff[x, y]) shade *= CliffTint;

        return (
            ClampToByte(baseColor.R * shade),
            ClampToByte(baseColor.G * shade),
            ClampToByte(baseColor.B * shade));
    }

    /// <summary>
    ///     Determines the unshaded base color of a cell.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="continentalness">Banded continentalness classification.</param>
    /// <param name="profile">World generation profile.</param>
    /// <param name="biomes">Classified biome map, or null for height-class coloring.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Base color.</returns>
    private static (byte R, byte G, byte B) BaseColorOf(TerrainMap map,
        ContinentalnessResult continentalness, WorldGenProfile profile, BiomeMap? biomes,
        int x, int y)
    {
        if (biomes is not null)
        {
            if (map.IsRiver[x, y] || map.IsLake[x, y]) return RiverColor;
            return BiomeColors[(int)biomes.Biome[x, y]];
        }

        if (map.IsRiver[x, y] || map.IsLake[x, y]) return RiverColor;
        if (map.IsBeach[x, y]) return BeachColor;

        if (map.IsOcean[x, y])
        {
            return continentalness.Classes[x, y] == ContinentalClass.DeepOcean
                ? DeepOceanColor
                : ShelfColor;
        }

        var h = map.Heights[x, y];
        var range = profile.SurfaceMaxZ - profile.SeaLevelZ;
        if (h >= profile.SeaLevelZ + range * 2 / 3) return MountainColor;
        if (h >= profile.SeaLevelZ + range / 3) return HighlandColor;
        return LowlandColor;
    }

    /// <summary>
    ///     Computes the hillshade multiplier of a cell from the northwest and southeast
    ///     height difference.
    /// </summary>
    /// <param name="map">Terrain map.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Hillshade multiplier.</returns>
    private static float HillshadeOf(TerrainMap map, int x, int y)
    {
        var nwX = Math.Max(x - 1, 0);
        var nwY = Math.Max(y - 1, 0);
        var seX = Math.Min(x + 1, map.Width - 1);
        var seY = Math.Min(y + 1, map.Height - 1);
        var delta = map.Heights[nwX, nwY] - map.Heights[seX, seY];

        return Math.Clamp(1f + HillshadeGain * delta, HillshadeMin, HillshadeMax);
    }

    /// <summary>
    ///     Downscales rendered pixels with box averaging.
    /// </summary>
    /// <param name="pixels">Rendered pixels.</param>
    /// <param name="width">Source width.</param>
    /// <param name="height">Source height.</param>
    /// <param name="outWidth">Output width.</param>
    /// <param name="outHeight">Output height.</param>
    /// <param name="factor">Box factor.</param>
    /// <returns>Downscaled pixels.</returns>
    internal static byte[] Downscale(byte[] pixels, int width, int height, int outWidth,
        int outHeight, int factor)
    {
        var output = new byte[outWidth * outHeight * 3];
        for (var oy = 0; oy < outHeight; ++oy)
        {
            for (var ox = 0; ox < outWidth; ++ox)
            {
                var r = 0;
                var g = 0;
                var b = 0;
                var count = 0;
                for (var dy = 0; dy < factor; ++dy)
                {
                    var sy = oy * factor + dy;
                    if (sy >= height) continue;

                    for (var dx = 0; dx < factor; ++dx)
                    {
                        var sx = ox * factor + dx;
                        if (sx >= width) continue;

                        var offset = (sy * width + sx) * 3;
                        r += pixels[offset];
                        g += pixels[offset + 1];
                        b += pixels[offset + 2];
                        ++count;
                    }
                }

                var outOffset = (oy * outWidth + ox) * 3;
                output[outOffset] = (byte)(r / count);
                output[outOffset + 1] = (byte)(g / count);
                output[outOffset + 2] = (byte)(b / count);
            }
        }

        return output;
    }

    /// <summary>
    ///     Clamps a color channel value to the byte range.
    /// </summary>
    /// <param name="value">Channel value.</param>
    /// <returns>Clamped channel value.</returns>
    private static byte ClampToByte(float value)
    {
        return (byte)Math.Clamp((int)value, 0, 255);
    }
}
