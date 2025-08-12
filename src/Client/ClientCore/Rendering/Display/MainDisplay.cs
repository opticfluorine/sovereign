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
using SDL3;
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
    private const string Title = "Sovereign";

    /// <summary>
    ///     Main window handle.
    /// </summary>
    public IntPtr WindowHandle { get; private set; }

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
        (SDL.GetWindowFlags(WindowHandle) & SDL.WindowFlags.InputFocus) != 0;

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
        WindowHandle = SDL.CreateWindow(Title, displayMode.Width, displayMode.Height, GetWindowFlags());
        SDL.SetWindowPosition(WindowHandle, (int)SDL.WindowPosCentered(), (int)SDL.WindowPosCentered());
    }

    /// <summary>
    ///     Closes the main window.
    /// </summary>
    public void Close()
    {
        SDL.DestroyWindow(WindowHandle);
    }

    /// <summary>
    ///     Gets the window flags to use.
    /// </summary>
    /// <returns></returns>
    private SDL.WindowFlags GetWindowFlags()
    {
        var flags = SDL.WindowFlags.Vulkan;
        if (IsFullscreen) flags |= SDL.WindowFlags.Fullscreen;
        return flags;
    }
}