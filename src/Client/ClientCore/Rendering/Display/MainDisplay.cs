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
using SDL2;
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering.Display;

/// <summary>
///     Manages the main display window.
/// </summary>
public class MainDisplay
{
    /// <summary>
    ///     Window title.
    /// </summary>
    private const string TITLE = "Sovereign";

    /// <summary>
    ///     Main window handle.
    /// </summary>
    public IntPtr WindowHandle { get; private set; }

    /// <summary>
    ///     Gets the window manager info associated with the main display.
    /// </summary>
    public SDL.SDL_SysWMinfo WMinfo
    {
        get
        {
            var info = new SDL.SDL_SysWMinfo();
            SDL.SDL_VERSION(out info.version);
            SDL.SDL_GetWindowWMInfo(WindowHandle, ref info);
            return info;
        }
    }

    /// <summary>
    ///     Display mode in use. Only valid after calling Show(), null otherwise.
    /// </summary>
    public IDisplayMode? DisplayMode { get; private set; }

    /// <summary>
    ///     Whether the display is set to fullscreen mode.
    /// </summary>
    public bool IsFullscreen { get; private set; }

    /// <summary>
    ///     Whether the window currently has input focus.
    /// </summary>
    public bool IsInputFocus =>
        (SDL.SDL_GetWindowFlags(WindowHandle)
         & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;

    /// <summary>
    ///     Shows the main window.
    /// </summary>
    /// <param name="displayMode">Display mode to use.</param>
    /// <param name="fullscreen">Whether to use fullscreen mode.</param>
    public void Show(IDisplayMode displayMode, bool fullscreen)
    {
        /* Configure fullscreen. */
        IsFullscreen = fullscreen;

        /* Create the main window. */
        DisplayMode = displayMode;
        WindowHandle = SDL.SDL_CreateWindow(TITLE,
            SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            displayMode.Width, displayMode.Height,
            GetWindowFlags());
    }

    /// <summary>
    ///     Closes the main window.
    /// </summary>
    public void Close()
    {
        SDL.SDL_DestroyWindow(WindowHandle);
    }

    private SDL.SDL_WindowFlags GetWindowFlags()
    {
        var flags = SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
        if (IsFullscreen) flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        return flags;
    }
}