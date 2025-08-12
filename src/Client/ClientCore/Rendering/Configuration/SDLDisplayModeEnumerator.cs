/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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
using System.Collections.Generic;
using SDL3;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Common display mode emulator using SDL2 functions.
/// </summary>
public class SDLDisplayModeEnumerator : IDisplayModeEnumerator
{
    public IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter)
    {
        // Query display modes from all displays.
        var displays = SDL.GetDisplays(out var displayCount) ?? throw new Exception("No displays found.");
        var modes = new List<SDLDisplayMode>();
        for (var i = 0U; i < displayCount; ++i)
        {
            var sdlModes = SDL.GetFullscreenDisplayModes(displays[i], out var modeCount);
            if (modeCount == 0) continue;
            if (sdlModes == null) throw new Exception("Null modes.");
            for (var j = 0; j < modeCount; ++j)
            {
                // Reject mode if we don't like the format.
                var mode = sdlModes[j];
                if (mode.Format != SDL.PixelFormat.XRGB8888) continue;

                // Otherwise add it to the list.
                modes.Add(new SDLDisplayMode(
                    mode.W,
                    mode.H,
                    DisplayFormat.B8G8R8A8_UNorm
                ));
            }
        }

        return modes;
    }

    /// <summary>
    ///     Converts an SDL format.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private DisplayFormat ConvertFormat(uint format)
    {
        throw new NotImplementedException();
    }
}