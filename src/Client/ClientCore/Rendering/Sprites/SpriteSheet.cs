/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
using System.Runtime.InteropServices;
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     A set of sprites loaded from a single file.
/// </summary>
public class SpriteSheet : IDisposable
{
    public SpriteSheet(Surface surface, SpriteSheetDefinition definition)
    {
        Surface = surface;
        Definition = definition;
    }

    /// <summary>
    ///     Spritesheet definition.
    /// </summary>
    public SpriteSheetDefinition Definition { get; }

    /// <summary>
    ///     SDL_Surface holding the spriteset.
    /// </summary>
    public Surface Surface { get; }

    public void Dispose()
    {
        Surface.Dispose();
    }

    /// <summary>
    ///     Checks the opacity of the sprite at the given row and column.
    /// </summary>
    /// <param name="row">Row.</param>
    /// <param name="column">Column.</param>
    /// <returns>true if the sprite is completely opaque; false otherwise.</returns>
    public bool CheckOpacity(int row, int column)
    {
        // We're going to test the opacity of the sprite by checking that every pixel has the
        // maximum alpha value. If every pixel has max alpha, then there is no transparency and
        // the renderer will not blend the sprite with the pixels beneath it; therefore, the
        // sprite is opaque.

        // Grab the raw pixel data for the sprite. Spritesheets are converted to the display format
        // when loaded, so we don't have to worry about conversions here. We're assuming 4 bytes per
        // pixel as these are the only display formats supported by the engine.
        var sheetPixelData = Surface.Properties.Data;
        var alphaMask = Surface.Properties.Format switch
        {
            DisplayFormat.B8G8R8A8_UNorm => 0xFF000000,
            DisplayFormat.R8G8B8A8_UNorm => 0xFF000000,
            _ => 0xFF000000
        };

        for (var x = column * Definition.SpriteWidth; x < (column + 1) * Definition.SpriteWidth; ++x)
        {
            for (var y = row * Definition.SpriteHeight; y < (row + 1) * Definition.SpriteHeight; ++y)
            {
                var byteOffset = y * Surface.Properties.Pitch + x * Surface.Properties.BytesPerPixel;
                var pixelValue = (uint)Marshal.ReadInt32(sheetPixelData, (int)byteOffset);
                if ((pixelValue & alphaMask) != alphaMask) return false;
            }
        }

        return true;
    }
}