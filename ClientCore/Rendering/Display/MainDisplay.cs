/*
 * Engine8 Dynamic World MMORPG Engine
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

using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Display
{

    /// <summary>
    /// Manages the main display window.
    /// </summary>
    public class MainDisplay
    {

        /// <summary>
        /// Default window width.
        /// </summary>
        private const int DEFAULT_WIDTH = 800;

        /// <summary>
        /// Default window height.
        /// </summary>
        private const int DEFAULT_HEIGHT = 600;

        /// <summary>
        /// Window title.
        /// </summary>
        private const string TITLE = "Engine8";

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

        public MainDisplay()
        {

        }

        /// <summary>
        /// Shows the main window.
        /// </summary>
        public void Show()
        {
            /* Create the main window. */
            windowHandle = SDL.SDL_CreateWindow(TITLE, 
                SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                DEFAULT_WIDTH, DEFAULT_HEIGHT, 
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        public void Close()
        {
            SDL.SDL_DestroyWindow(windowHandle);
        }

    }

}
