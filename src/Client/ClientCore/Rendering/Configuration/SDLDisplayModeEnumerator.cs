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
using SDL2;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Common display mode emulator using SDL2 functions.
/// </summary>
public class SDLDisplayModeEnumerator : IDisplayModeEnumerator
{
    public IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter)
    {
        // Query display modes from all displays.
        var displayCount = SDL.SDL_GetNumVideoDisplays();
        var modes = new List<SDLDisplayMode>();
        for (var i = 0; i < displayCount; ++i)
        {
            var modeCount = SDL.SDL_GetNumDisplayModes(i);
            for (var j = 0; j < modeCount; ++j)
            {
                SDL.SDL_GetDisplayMode(i, j, out var mode);

                // Reject mode if we don't like the format.
                // TODO Support more formats... or do we care?
                if (mode.format != SDL.SDL_PIXELFORMAT_RGB888) continue;

                // Otherwise add it to the list.
                modes.Add(new SDLDisplayMode(
                    mode.w,
                    mode.h,
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