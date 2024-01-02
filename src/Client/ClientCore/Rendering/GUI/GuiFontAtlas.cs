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
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering.GUI;

/// <summary>
///     Manages the GUI font atlas as an SDL surface.
/// </summary>
public sealed class GuiFontAtlas : IDisposable
{
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
        // Retrieve raw data from ImGui.
        ImGui.GetIO().Fonts.GetTexDataAsRGBA32(out IntPtr outPixels,
            out var outWidth, out var outHeight);
        width = outWidth;
        height = outHeight;

        // Create an SDL_Surface to hold the atlas.
        fontAtlasSurface = Surface.CreateSurfaceFrom(outPixels, outWidth, outHeight,
            DisplayFormat.B8G8R8A8_UNorm);
    }
}