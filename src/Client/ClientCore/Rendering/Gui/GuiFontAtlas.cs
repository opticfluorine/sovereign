/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Manages the GUI font atlas as an SDL surface.
/// </summary>
public sealed class GuiFontAtlas : IDisposable
{
    /// <summary>
    ///     ImGui texture ID for the font atlas.
    /// </summary>
    public static readonly IntPtr TextureId = 1;

    private readonly DisplayOptions displayOptions;

    private readonly MainDisplay mainDisplay;
    private readonly IResourcePathBuilder resourcePathBuilder;

    /// <summary>
    ///     Pointer to the SDL surface containing the font atlas.
    /// </summary>
    private Surface? fontAtlasSurface;

    /// <summary>
    ///     Height.
    /// </summary>
    private int height;

    /// <summary>
    ///     Width.
    /// </summary>
    private int width;

    public GuiFontAtlas(IResourcePathBuilder resourcePathBuilder,
        MainDisplay mainDisplay, IOptions<DisplayOptions> displayOptions)
    {
        this.resourcePathBuilder = resourcePathBuilder;
        this.mainDisplay = mainDisplay;
        this.displayOptions = displayOptions.Value;
    }

    /// <summary>
    ///     Gets a pointer to the SDL surface containing the font atlas.
    /// </summary>
    public Surface FontAtlasSurface
    {
        get
        {
            if (fontAtlasSurface == null) InitializeSurface();

            return fontAtlasSurface;
        }
    }

    /// <summary>
    ///     Width of the font atlas, in pixels.
    /// </summary>
    public int Width
    {
        get
        {
            if (fontAtlasSurface == null) InitializeSurface();

            return width;
        }
    }

    /// <summary>
    ///     Height of the font atlas, in pixels.
    /// </summary>
    public int Height
    {
        get
        {
            if (fontAtlasSurface == null) InitializeSurface();

            return height;
        }
    }

    public void Dispose()
    {
        fontAtlasSurface?.Dispose();
    }

    /// <summary>
    ///     Initializes the font atlas surface.
    /// </summary>
    [MemberNotNull("fontAtlasSurface")]
    private void InitializeSurface()
    {
        // Determine UI scaling with resolution.
        var scaleFactor = (float)mainDisplay.DisplayMode!.Height /
                          displayOptions.BaseScalingHeight;
        var fontSize = scaleFactor * displayOptions.BaseFontSize;

        // Load font.
        var fontPath =
            resourcePathBuilder.BuildPathToResource(ResourceType.Fonts, displayOptions.Font);
        var io = ImGui.GetIO();
        io.Fonts.AddFontFromFileTTF(fontPath, fontSize, null, io.Fonts.GetGlyphRangesDefault());
        io.Fonts.Build();

        // Retrieve raw data from ImGui.
        io.Fonts.GetTexDataAsRGBA32(out IntPtr outPixels,
            out var outWidth, out var outHeight);
        width = outWidth;
        height = outHeight;

        // Create an SDL_Surface to hold the atlas.
        fontAtlasSurface = Surface.CreateSurfaceFrom(outPixels, outWidth, outHeight,
            DisplayFormat.B8G8R8A8_UNorm);

        // Tag the font texture for later lookup.
        io.Fonts.SetTexID(TextureId);
    }
}