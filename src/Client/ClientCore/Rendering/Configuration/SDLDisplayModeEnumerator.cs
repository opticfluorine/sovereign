/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using SDL2;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
/// Common display mode emulator using SDL2 functions.
/// </summary>
public class SDLDisplayModeEnumerator : IDisplayModeEnumerator
{

    public IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter)
    {
        // Query display modes from all displays.
        int displayCount = SDL.SDL_GetNumVideoDisplays();
        var modes = new List<SDLDisplayMode>();
        for (int i = 0; i < displayCount; ++i)
        {
            int modeCount = SDL.SDL_GetNumDisplayModes(i);
            for (int j = 0; j < modeCount; ++j)
            {
                SDL.SDL_GetDisplayMode(i, j, out var mode);

                // Reject mode if we don't like the format.
                // TODO Support more formats... or do we care?
                if (mode.format != SDL.SDL_PIXELFORMAT_RGB888) continue;

                // Otherwise add it to the list.
                modes.Add(new SDLDisplayMode(
                    width: mode.w,
                    height: mode.h,
                    format: DisplayFormat.B8G8R8A8_UNorm
                ));
            }
        }

        return modes;
    }

    /// <summary>
    /// Converts an SDL format.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private DisplayFormat ConvertFormat(uint format)
    {
        throw new NotImplementedException();
    }
}
