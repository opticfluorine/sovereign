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

namespace Engine8.ClientCore.Systems.Input
{

    /// <summary>
    /// Tracks the state of the keyboard.
    /// </summary>
    public class KeyboardState
    {

        /// <summary>
        /// Keys that are currently pressed.
        /// </summary>
        private IDictionary<SDL.SDL_Keycode, bool> keysDown { get; } 
            = new Dictionary<SDL.SDL_Keycode, bool>();

        /// <summary>
        /// Indicates whether the given key is currently pressed.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>true if pressed, false otherwise.</returns>
        public bool this[SDL.SDL_Keycode key]
        {
            get => keysDown.ContainsKey(key) ? keysDown[key] : false;
        }

        /// <summary>
        /// Registers that a key has been pressed.
        /// </summary>
        /// <param name="key">Key that was pressed.</param>
        public void KeyDown(SDL.SDL_Keycode key)
        {
            keysDown[key] = true;
        }

        /// <summary>
        /// Registers that a key has been released.
        /// </summary>
        /// <param name="key">Key that was released.</param>
        public void KeyUp(SDL.SDL_Keycode key)
        {
            keysDown[key] = false;
        }

    }

}
