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
using Sovereign.WorldGen.Caves;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Renders top-down preview images of a cave system, one per cave level. Open cells
///     are dark gray, floor-shaded, with corridors and chambers distinguished through the
///     Worley distance; shafts are circle markers colored by level pair; mouths are red
///     rings on the shallowest level's image; the water-exclusion zone is shaded faint
///     blue. The previews are diagnostic images, not art.
/// </summary>
public sealed class CavePreviewRenderer
{
    /// <summary>
    ///     Maximum preview dimension in pixels.
    /// </summary>
    private const int MaxPreviewDimension = 1024;

    /// <summary>
    ///     Radius in pixels of the shaft circle markers, scaled with the image.
    /// </summary>
    private const int ShaftMarkerRadius = 3;

    /// <summary>
    ///     Radius in pixels of the mouth ring markers, scaled with the image.
    /// </summary>
    private const int MouthRingRadius = 4;

    /// <summary>
    ///     Solid rock color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) RockColor = (22, 22, 26);

    /// <summary>
    ///     Faint blue tint for the water-exclusion zone.
    /// </summary>
    private static readonly (byte R, byte G, byte B) WaterExclusionTint = (24, 34, 54);

    /// <summary>
    ///     Mouth ring color.
    /// </summary>
    private static readonly (byte R, byte G, byte B) MouthRingColor = (220, 40, 40);

    /// <summary>
    ///     Marker colors per level pair index.
    /// </summary>
    private static readonly (byte R, byte G, byte B)[] PairColors =
    {
        (240, 200, 80),
        (120, 220, 120),
        (120, 180, 240),
        (230, 140, 220),
        (240, 150, 90),
        (160, 230, 220)
    };

    /// <summary>
    ///     Renders one preview image per cave level, in top-to-bottom level order.
    /// </summary>
    /// <param name="caves">Carved cave map.</param>
    /// <returns>Preview images in level order.</returns>
    public IReadOnlyList<PreviewImage> Render(CaveMap caves)
    {
        var images = new List<PreviewImage>(caves.Levels.Count);
        for (var i = 0; i < caves.Levels.Count; ++i)
        {
            images.Add(RenderLevel(caves, i));
        }

        return images;
    }

    /// <summary>
    ///     Renders the preview image of one cave level.
    /// </summary>
    /// <param name="caves">Carved cave map.</param>
    /// <param name="level">Level index.</param>
    /// <returns>Rendered preview image.</returns>
    private PreviewImage RenderLevel(CaveMap caves, int level)
    {
        var levelMap = caves.Levels[level];
        var factor = Math.Max(caves.Width, caves.Height) <= MaxPreviewDimension
            ? 1
            : (Math.Max(caves.Width, caves.Height) + MaxPreviewDimension - 1) / MaxPreviewDimension;
        var outWidth = (caves.Width + factor - 1) / factor;
        var outHeight = (caves.Height + factor - 1) / factor;

        var pixels = new byte[caves.Width * caves.Height * 3];
        for (var y = 0; y < caves.Height; ++y)
        {
            for (var x = 0; x < caves.Width; ++x)
            {
                var color = ColorOf(caves, level, x, y);
                var offset = (y * caves.Width + x) * 3;
                pixels[offset] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.B;
            }
        }

        if (factor > 1)
        {
            pixels = PreviewRenderer.Downscale(pixels, caves.Width, caves.Height, outWidth,
                outHeight, factor);
        }

        DrawMarkers(caves, level, pixels, outWidth, outHeight, factor);
        return new PreviewImage { Width = outWidth, Height = outHeight, Pixels = pixels };
    }

    /// <summary>
    ///     Determines the color of a cave cell.
    /// </summary>
    /// <param name="caves">Carved cave map.</param>
    /// <param name="level">Level index.</param>
    /// <param name="x">Cell X coordinate.</param>
    /// <param name="y">Cell Y coordinate.</param>
    /// <returns>Cell color.</returns>
    private static (byte R, byte G, byte B) ColorOf(CaveMap caves, int level, int x, int y)
    {
        var levelMap = caves.Levels[level];
        if (levelMap.Open[x, y])
        {
            // Dark gray floor shading by floor Z, tinted by the Worley distance so that
            // corridors (high F2-F1) and chambers (low F2-F1) are visually distinct.
            var span = 2 * 2;
            var floorT = (levelMap.FloorZ[x, y] - (levelMap.BaseFloorZ - 2)) / (float)span;
            var worleyT = levelMap.Worley[x, y] / 255f;
            var shade = 0.75f + 0.25f * Math.Clamp(floorT, 0f, 1f);
            var channel = (int)((70 + 50 * worleyT) * shade);
            return ((byte)channel, (byte)channel, (byte)(channel + 6));
        }

        return levelMap.WaterExcluded[x, y] ? WaterExclusionTint : RockColor;
    }

    /// <summary>
    ///     Draws the shaft circle markers and mouth rings on a rendered level image.
    /// </summary>
    /// <param name="caves">Carved cave map.</param>
    /// <param name="level">Level index.</param>
    /// <param name="pixels">Rendered pixels, updated in place.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="factor">Box downscale factor applied to the image.</param>
    private static void DrawMarkers(CaveMap caves, int level, byte[] pixels, int width,
        int height, int factor)
    {
        var pairIndex = 0;
        foreach (var shaft in caves.Shafts)
        {
            if (shaft.UpperLevel != level && shaft.LowerLevel != level) continue;

            DrawCircle(pixels, width, height,
                shaft.CenterX / factor, shaft.CenterY / factor, ShaftMarkerRadius,
                PairColors[pairIndex % PairColors.Length]);
            ++pairIndex;
        }

        if (level != 0) return;

        foreach (var mouth in caves.MouthList)
        {
            DrawCircle(pixels, width, height,
                (mouth.X + 1) / factor, (mouth.Y + 1) / factor, MouthRingRadius,
                MouthRingColor);
        }
    }

    /// <summary>
    ///     Draws a filled circle marker.
    /// </summary>
    /// <param name="pixels">Image pixels, updated in place.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="cx">Center X pixel coordinate.</param>
    /// <param name="cy">Center Y pixel coordinate.</param>
    /// <param name="radius">Circle radius in pixels.</param>
    /// <param name="color">Circle color.</param>
    private static void DrawCircle(byte[] pixels, int width, int height, int cx, int cy,
        int radius, (byte R, byte G, byte B) color)
    {
        for (var dy = -radius; dy <= radius; ++dy)
        {
            for (var dx = -radius; dx <= radius; ++dx)
            {
                if (dx * dx + dy * dy > radius * radius) continue;

                var x = cx + dx;
                var y = cy + dy;
                if (x < 0 || y < 0 || x >= width || y >= height) continue;

                var offset = (y * width + x) * 3;
                pixels[offset] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.B;
            }
        }
    }
}
