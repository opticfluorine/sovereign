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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Linear RGBA8 color used by the vitals bar pixel generator.
/// </summary>
/// <param name="Red">Red channel.</param>
/// <param name="Green">Green channel.</param>
/// <param name="Blue">Blue channel.</param>
/// <param name="Alpha">Alpha channel.</param>
public readonly record struct VitalsBarColor(byte Red, byte Green, byte Blue, byte Alpha)
{
    /// <summary>
    ///     Writes the color to a single pixel in RGBA8 byte order.
    /// </summary>
    /// <param name="pixel">Four-byte destination span.</param>
    internal void WriteTo(Span<byte> pixel)
    {
        pixel[0] = Red;
        pixel[1] = Green;
        pixel[2] = Blue;
        pixel[3] = Alpha;
    }
}

/// <summary>
///     Pure pixel generator for vitals HUD progress bars.
/// </summary>
/// <remarks>
///     <para>
///         Produces a tightly packed row-major linear RGBA8 pixel buffer depicting a
///         pixel-art progress bar: a one-pixel solid near-black border, a dark translucent
///         interior background, and a solid fill inset by one pixel on all sides.
///     </para>
///     <para>
///         This class is deliberately free of ImGui, Veldrid, and texture manager
///         dependencies so that the pixel generation can be unit-tested without a GPU.
///     </para>
/// </remarks>
public static class VitalsBarPixels
{
    /// <summary>
    ///     Border thickness in pixels on all sides of the bar.
    /// </summary>
    public const int BorderWidth = 1;

    /// <summary>
    ///     Solid near-black border color.
    /// </summary>
    public static readonly VitalsBarColor BorderColor = new(12, 10, 16, byte.MaxValue);

    /// <summary>
    ///     Dark translucent interior background color.
    /// </summary>
    public static readonly VitalsBarColor BackgroundColor = new(16, 14, 20, 140);

    /// <summary>
    ///     Creates an opaque fill color from a base color.
    /// </summary>
    /// <param name="red">Red channel.</param>
    /// <param name="green">Green channel.</param>
    /// <param name="blue">Blue channel.</param>
    /// <returns>Opaque fill color.</returns>
    public static VitalsBarColor MakeFillColor(byte red, byte green, byte blue)
    {
        return new VitalsBarColor(red, green, blue, byte.MaxValue);
    }

    /// <summary>
    ///     Fills a tightly packed row-major linear RGBA8 pixel buffer with a progress bar.
    /// </summary>
    /// <param name="pixels">Pixel buffer, exactly width * height * 4 bytes.</param>
    /// <param name="width">Bar width in pixels.</param>
    /// <param name="height">Bar height in pixels.</param>
    /// <param name="fraction">Fill fraction in [0, 1]; out-of-range values are clamped.</param>
    /// <param name="fillColor">Solid fill color for the bar interior.</param>
    /// <exception cref="ArgumentOutOfRangeException">If a dimension is too small for the border.</exception>
    /// <exception cref="ArgumentException">If the pixel buffer has the wrong length.</exception>
    public static void FillBar(Span<byte> pixels, int width, int height, float fraction,
        VitalsBarColor fillColor)
    {
        ValidateDimensions(pixels, width, height);

        var fillPixelWidth = GetFillPixelWidth(width, fraction);
        var borderColor = BorderColor;
        var backgroundColor = BackgroundColor;
        for (var y = 0; y < height; ++y)
        {
            var rowStart = y * width;
            var isBorderRow = y < BorderWidth || y >= height - BorderWidth;
            for (var x = 0; x < width; ++x)
            {
                var pixel = pixels.Slice((rowStart + x) * 4);
                if (isBorderRow || x < BorderWidth || x >= width - BorderWidth)
                    borderColor.WriteTo(pixel);
                else if (x - BorderWidth < fillPixelWidth)
                    fillColor.WriteTo(pixel);
                else
                    backgroundColor.WriteTo(pixel);
            }
        }
    }

    /// <summary>
    ///     Gets the fill width in interior pixels for a fill fraction, rounding down to
    ///     the pixel but keeping at least one fill pixel when the fraction is positive.
    /// </summary>
    /// <param name="width">Bar width in pixels.</param>
    /// <param name="fraction">Fill fraction; out-of-range values are clamped.</param>
    /// <returns>Fill width in pixels in [0, width - 2 * BorderWidth].</returns>
    public static int GetFillPixelWidth(int width, float fraction)
    {
        var interiorWidth = GetInteriorWidth(width);
        if (interiorWidth <= 0) return 0;

        var clamped = Math.Clamp(fraction, 0.0f, 1.0f);
        if (clamped <= 0.0f) return 0;

        var fillPixelWidth = (int)(clamped * interiorWidth);
        return Math.Clamp(fillPixelWidth, 1, interiorWidth);
    }

    /// <summary>
    ///     Quantizes a fill fraction to the pixel resolution of a bar of the given width,
    ///     for cheap change detection between dynamic texture uploads.
    /// </summary>
    /// <param name="width">Bar width in pixels.</param>
    /// <param name="fraction">Fill fraction; out-of-range values are clamped.</param>
    /// <returns>Quantized fraction in [0, 1].</returns>
    public static float QuantizeFraction(int width, float fraction)
    {
        var interiorWidth = GetInteriorWidth(width);
        if (interiorWidth <= 0) return 0.0f;

        return GetFillPixelWidth(width, fraction) / (float)interiorWidth;
    }

    /// <summary>
    ///     Gets the interior width in pixels inside the border.
    /// </summary>
    /// <param name="width">Bar width in pixels.</param>
    /// <returns>Interior width in pixels.</returns>
    private static int GetInteriorWidth(int width)
    {
        return width - 2 * BorderWidth;
    }

    /// <summary>
    ///     Validates the bar dimensions and pixel buffer length.
    /// </summary>
    /// <param name="pixels">Pixel buffer.</param>
    /// <param name="width">Bar width in pixels.</param>
    /// <param name="height">Bar height in pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">If a dimension is too small for the border.</exception>
    /// <exception cref="ArgumentException">If the pixel buffer has the wrong length.</exception>
    private static void ValidateDimensions(ReadOnlySpan<byte> pixels, int width, int height)
    {
        if (width < 2 * BorderWidth + 1)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"Bar width must exceed twice the border width ({2 * BorderWidth}).");
        if (height < 2 * BorderWidth + 1)
            throw new ArgumentOutOfRangeException(nameof(height),
                $"Bar height must exceed twice the border width ({2 * BorderWidth}).");
        if (pixels.Length != width * height * 4)
            throw new ArgumentException(
                $"Pixel buffer must be exactly {width * height * 4} bytes, got {pixels.Length}.",
                nameof(pixels));
    }
}
