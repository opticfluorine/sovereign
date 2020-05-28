/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Rendering.Configuration;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sovereign.ClientCore.Events;

namespace Sovereign.ClientCore.Rendering.Display
{

    /// <summary>
    /// Manages the main display window.
    /// </summary>
    public class MainDisplay
    {

        /// <summary>
        /// Window title.
        /// </summary>
        private const string TITLE = "Sovereign";

        /// <summary>
        /// Main window handle.
        /// </summary>
        public IntPtr WindowHandle { get; private set; }

        /// <summary>
        /// The HWND associated with the created window.
        /// </summary>
        public IntPtr WindowHwnd
        {
            get
            {
                SDL.SDL_SysWMinfo info = new SDL.SDL_SysWMinfo();
                SDL.SDL_GetWindowWMInfo(WindowHandle, ref info);
                return info.info.win.window;
            }
        }

        /// <summary>
        /// Display mode in use. Only valid after calling Show().
        /// </summary>
        public IDisplayMode DisplayMode { get; private set; }

        /// <summary>
        /// Whether the display is set to fullscreen mode.
        /// </summary>
        public bool IsFullscreen { get; private set; }

        /// <summary>
        /// Whether the window currently has input focus.
        /// </summary>
        public bool IsInputFocus =>
            (SDL.SDL_GetWindowFlags(WindowHandle)
             & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;

        /// <summary>
        /// Shows the main window.
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
        /// Closes the main window.
        /// </summary>
        public void Close()
        {
            SDL.SDL_DestroyWindow(WindowHandle);
        }

        private SDL.SDL_WindowFlags GetWindowFlags()
        {
            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
            if (IsFullscreen)
            {
                flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
            }
            return flags;
        }

    }

}
