/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
        private IntPtr windowHandle;

        /// <summary>
        /// The HWND associated with the created window.
        /// </summary>
        public IntPtr WindowHwnd
        {
            get
            {
                SDL.SDL_SysWMinfo info = new SDL.SDL_SysWMinfo();
                SDL.SDL_GetWindowWMInfo(windowHandle, ref info);
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
        /// Creates the display.
        /// </summary>
        /// <param name="eventAdapter">
        /// SDL event adapter. It is anchored here for IoC resolution since
        /// the SDL library is managed outside of the object graph.
        /// </param>
        public MainDisplay(SDLEventAdapter eventAdapter)
        {

        }

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
            windowHandle = SDL.SDL_CreateWindow(TITLE, 
                SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                displayMode.Width, displayMode.Height, 
                GetWindowFlags());
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        public void Close()
        {
            SDL.SDL_DestroyWindow(windowHandle);
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
