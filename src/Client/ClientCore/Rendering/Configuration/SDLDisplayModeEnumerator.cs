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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SDL2;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Common display mode emulator using SDL2 functions.
/// </summary>
public class SDLDisplayModeEnumerator(ILogger<SDLDisplayModeEnumerator> logger) : IDisplayModeEnumerator
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
                if (mode.format != SDL.SDL_PIXELFORMAT_RGB888) continue;

                // Otherwise add it to the list.
                logger.LogDebug("Available mode: {W}x{H}", mode.w, mode.h);
                modes.Add(new SDLDisplayMode(
                    mode.w,
                    mode.h,
                    DisplayFormat.B8G8R8A8_UNorm
                ));
            }
        }

        return modes;
    }
}