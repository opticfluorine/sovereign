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

using SFML.Graphics;
using SFML.System;
using SFML.Window;
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
        private const uint WIDTH = 1366;

        /// <summary>
        /// Default window height.
        /// </summary>
        private const uint HEIGHT = 768;

        /// <summary>
        /// 
        /// </summary>
        private static readonly Styles STYLES = Styles.Close;

        /// <summary>
        /// Window title.
        /// </summary>
        private const string TITLE = "Engine8";

        /// <summary>
        /// Main render window.
        /// </summary>
        public RenderWindow RenderWindow { get; private set; }

        public MainDisplay()
        {
            /* Create and configure the main window. */
            var mode = new VideoMode(WIDTH, HEIGHT);
            RenderWindow = new RenderWindow(mode, TITLE, STYLES);
        }

        /// <summary>
        /// Shows the main window.
        /// </summary>
        public void Show()
        {
            RenderWindow.Display();
        }

        public void Close()
        {
            RenderWindow.Close();
        }

    }

}
