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
using Hexa.NET.ImGui;
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
    private const uint FirstIconCodePoint = 0xe000;
    private const uint LastIconCodePoint = 0xe0fe;
    private const uint FirstEmojiCodePoint = 0x1;
    private const uint LastEmojiCodePoint = 0x1ffff;

    /// <summary>
    ///     ImGui texture ID for the font atlas.
    /// </summary>
    public static readonly ImTextureID TextureId = 1;

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
            if (fontAtlasSurface == null) throw new InvalidOperationException("GUI font atlas not initialized.");
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
            if (fontAtlasSurface == null) throw new InvalidOperationException("GUI font atlas not initialized.");
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
            if (fontAtlasSurface == null) throw new InvalidOperationException("GUI font atlas not initialized.");
            return height;
        }
    }

    public void Dispose()
    {
        fontAtlasSurface?.Dispose();
    }

    /// <summary>
    ///     Initializes the font atlas.
    /// </summary>
    public void Initialize()
    {
        InitializeSurface();
    }

    /// <summary>
    ///     Initializes the font atlas surface.
    /// </summary>
    [MemberNotNull("fontAtlasSurface")]
    private unsafe void InitializeSurface()
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

        // Load icon font.
        // This assumes the use of OpenFontIcons (https://github.com/traverseda/OpenFontIcons).
        // If you use a different icon font, adjust the code points accordingly (here and in IconCodePoints).
        var iconRange = stackalloc uint[] { FirstIconCodePoint, LastIconCodePoint, 0 /* list terminator */ };
        var iconFontPath =
            resourcePathBuilder.BuildPathToResource(ResourceType.Fonts, displayOptions.IconFont);
        var iconConfig = stackalloc ImFontConfig[1];
        iconConfig[0] = new ImFontConfig
        {
            MergeMode = 1, // should be true, but ImGui.NET uses byte instead of bool here
            GlyphMinAdvanceX = fontSize, // load font as monospace

            // Following fields are normally set by the ImFontConfig constructor in C++, not in ImGUI.NET though.
            FontDataOwnedByAtlas = 1,
            OversampleH = 1,
            OversampleV = 1,
            GlyphMaxAdvanceX = float.MaxValue,
            RasterizerMultiply = 1.0f,
            RasterizerDensity = 1.0f,
            EllipsisChar = 0
        };

        io.Fonts.AddFontFromFileTTF(iconFontPath, fontSize, iconConfig, iconRange);

        // Load emojis.
        var emojiRange = stackalloc uint[] { FirstEmojiCodePoint, LastEmojiCodePoint, 0 /* list terminator */ };
        var emojiFontPath =
            resourcePathBuilder.BuildPathToResource(ResourceType.Fonts, displayOptions.EmojiFont);
        var emojiConfig = stackalloc ImFontConfig[1];
        emojiConfig[0] = new ImFontConfig
        {
            MergeMode = 1,
            FontBuilderFlags = (uint)ImGuiFreeTypeBuilderFlags.LoadColor,

            // Following fields are normally set by the ImFontConfig constructor in C++, not in ImGUI.NET though.
            FontDataOwnedByAtlas = 1,
            OversampleH = 1,
            OversampleV = 1,
            GlyphMaxAdvanceX = float.MaxValue,
            RasterizerMultiply = 1.0f,
            RasterizerDensity = 1.0f,
            EllipsisChar = 0
        };

        io.Fonts.AddFontFromFileTTF(emojiFontPath, fontSize, emojiConfig, emojiRange);

        // Retrieve raw data from ImGui.
        io.Fonts.Build();
        var outPixels = stackalloc byte*[1];
        io.Fonts.GetTexDataAsRGBA32(outPixels, ref width, ref height);

        // Create an SDL_Surface to hold the atlas.
        fontAtlasSurface = Surface.CreateSurfaceFrom(new IntPtr(*outPixels), width, height,
            DisplayFormat.R8G8B8A8_UNorm);

        // Tag the font texture for later lookup.
        io.Fonts.SetTexID(TextureId);
    }
}