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

using System.Collections.Generic;
using SDL2;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Tracks the state of the keyboard.
/// </summary>
public class KeyboardState
{
    /// <summary>
    ///     Keys that are currently pressed.
    /// </summary>
    private IDictionary<SDL.SDL_Keycode, bool> keysDown { get; }
        = new Dictionary<SDL.SDL_Keycode, bool>();

    /// <summary>
    ///     Indicates whether the given key is currently pressed.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>true if pressed, false otherwise.</returns>
    public bool this[SDL.SDL_Keycode key] => keysDown.ContainsKey(key) ? keysDown[key] : false;

    /// <summary>
    ///     Registers that a key has been pressed.
    /// </summary>
    /// <param name="key">Key that was pressed.</param>
    public void KeyDown(SDL.SDL_Keycode key)
    {
        keysDown[key] = true;
    }

    /// <summary>
    ///     Registers that a key has been released.
    /// </summary>
    /// <param name="key">Key that was released.</param>
    public void KeyUp(SDL.SDL_Keycode key)
    {
        keysDown[key] = false;
    }
}