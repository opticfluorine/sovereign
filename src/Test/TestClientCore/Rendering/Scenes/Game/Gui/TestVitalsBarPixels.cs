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
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui;
using Xunit;

namespace TestClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Unit tests for <see cref="VitalsBarPixels"/>.
/// </summary>
public class TestVitalsBarPixels
{
    private const int Width = 16;
    private const int Height = 8;
    private const int InteriorWidth = Width - 2 * VitalsBarPixels.BorderWidth;
    private const int InteriorHeight = Height - 2 * VitalsBarPixels.BorderWidth;

    private static readonly VitalsBarColor FillColor = VitalsBarPixels.MakeFillColor(200, 40, 40);

    [Fact]
    public void FillBar_RequiresExactBufferSize()
    {
        var pixels = new byte[Width * Height * 4 - 1];
        Assert.Throws<ArgumentException>(() =>
            VitalsBarPixels.FillBar(pixels, Width, Height, 0.5f, FillColor));

        var exactBuffer = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(exactBuffer, Width, Height, 0.5f, FillColor);
    }

    [Fact]
    public void FillBar_RejectsDimensionsTooSmallForBorder()
    {
        var pixels = new byte[4 * 4 * 4];
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VitalsBarPixels.FillBar(pixels, 2, 8, 0.5f, FillColor));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VitalsBarPixels.FillBar(pixels, 8, 2, 0.5f, FillColor));
    }

    [Fact]
    public void FillBar_ZeroFractionDrawsBorderAndEmptyBackground()
    {
        var pixels = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(pixels, Width, Height, 0.0f, FillColor);

        // Border is drawn on all four edges.
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, 0, 0));
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, Width - 1, 0));
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, 0, Height - 1));
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, Width - 1, Height - 1));

        // Interior is entirely translucent background with no fill.
        for (var y = 1; y < Height - 1; ++y)
        {
            for (var x = 1; x < Width - 1; ++x)
            {
                Assert.Equal(VitalsBarPixels.BackgroundColor, PixelAt(pixels, x, y));
            }
        }
    }

    [Fact]
    public void FillBar_FullFractionDrawsSolidFillWithBorder()
    {
        var pixels = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(pixels, Width, Height, 1.0f, FillColor);

        // Border is drawn on all four edges.
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, 0, 0));
        Assert.Equal(VitalsBarPixels.BorderColor, PixelAt(pixels, Width - 1, Height - 1));

        // Interior is entirely solid fill.
        for (var y = 1; y < Height - 1; ++y)
        {
            for (var x = 1; x < Width - 1; ++x)
            {
                Assert.Equal(FillColor, PixelAt(pixels, x, y));
            }
        }
    }

    [Fact]
    public void FillBar_PartialFractionRoundsDownToPixel()
    {
        var fraction = 0.29f; // 0.29 * InteriorWidth = 4.06 fill pixels -> 4.
        var expectedFillWidth = (int)(fraction * InteriorWidth);
        Assert.InRange(expectedFillWidth, 1, InteriorWidth - 1);

        var pixels = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(pixels, Width, Height, fraction, FillColor);

        // Row-major layout: interior fill region starts just inside the border.
        Assert.Equal(FillColor, PixelAt(pixels, 1, 1));
        Assert.Equal(FillColor, PixelAt(pixels, expectedFillWidth, 1));
        Assert.Equal(VitalsBarPixels.BackgroundColor, PixelAt(pixels, expectedFillWidth + 1, 1));
        Assert.Equal(VitalsBarPixels.BackgroundColor, PixelAt(pixels, Width - 2, 1));

        // Every interior row is filled identically.
        Assert.Equal(FillColor, PixelAt(pixels, 1, InteriorHeight));
        Assert.Equal(VitalsBarPixels.BackgroundColor, PixelAt(pixels, expectedFillWidth + 1, InteriorHeight));
    }

    [Fact]
    public void FillBar_TinyPositiveFractionKeepsSingleFillPixel()
    {
        var pixels = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(pixels, Width, Height, 0.001f, FillColor);

        Assert.Equal(FillColor, PixelAt(pixels, 1, 1));
        Assert.Equal(VitalsBarPixels.BackgroundColor, PixelAt(pixels, 2, 1));
    }

    [Fact]
    public void FillBar_ClampsOutOfRangeFractions()
    {
        var empty = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(empty, Width, Height, 0.0f, FillColor);

        var negative = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(negative, Width, Height, -0.5f, FillColor);
        Assert.Equal(empty, negative);

        var full = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(full, Width, Height, 1.0f, FillColor);

        var overfull = new byte[Width * Height * 4];
        VitalsBarPixels.FillBar(overfull, Width, Height, 1.5f, FillColor);
        Assert.Equal(full, overfull);
    }

    [Fact]
    public void GetFillPixelWidth_RoundsDownAndClampsToOnePixelWhenPositive()
    {
        Assert.Equal(0, VitalsBarPixels.GetFillPixelWidth(Width, 0.0f));
        Assert.Equal(1, VitalsBarPixels.GetFillPixelWidth(Width, 0.001f));
        Assert.Equal((int)(0.5f * InteriorWidth), VitalsBarPixels.GetFillPixelWidth(Width, 0.5f));
        Assert.Equal(InteriorWidth, VitalsBarPixels.GetFillPixelWidth(Width, 1.0f));
        Assert.Equal(0, VitalsBarPixels.GetFillPixelWidth(Width, -0.25f));
        Assert.Equal(InteriorWidth, VitalsBarPixels.GetFillPixelWidth(Width, 2.0f));
    }

    [Fact]
    public void QuantizeFraction_MatchesFillPixelWidthRatio()
    {
        Assert.Equal(0.0f, VitalsBarPixels.QuantizeFraction(Width, 0.0f));
        Assert.Equal(1.0f, VitalsBarPixels.QuantizeFraction(Width, 1.0f));
        Assert.Equal(1.0f / InteriorWidth, VitalsBarPixels.QuantizeFraction(Width, 0.001f));

        for (var fillPixels = 0; fillPixels <= InteriorWidth; ++fillPixels)
        {
            var fraction = fillPixels / (float)InteriorWidth;
            Assert.Equal(fillPixels / (float)InteriorWidth,
                VitalsBarPixels.QuantizeFraction(Width, fraction));
        }
    }

    /// <summary>
    ///     Gets the color of one pixel in a generated bar buffer.
    /// </summary>
    /// <param name="pixels">Pixel buffer.</param>
    /// <param name="x">X coordinate in pixels.</param>
    /// <param name="y">Y coordinate in pixels.</param>
    /// <returns>Pixel color.</returns>
    private static VitalsBarColor PixelAt(byte[] pixels, int x, int y)
    {
        var offset = (y * Width + x) * 4;
        return new VitalsBarColor(pixels[offset], pixels[offset + 1], pixels[offset + 2],
            pixels[offset + 3]);
    }
}
