// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Numerics;
using SDL2;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Public read API for the Input system.
/// </summary>
public class InputServices
{
    private readonly KeyboardState keyboardState;
    private readonly MouseState mouseState;

    public InputServices(MouseState mouseState, KeyboardState keyboardState)
    {
        this.mouseState = mouseState;
        this.keyboardState = keyboardState;
    }

    /// <summary>
    ///     Gets the latest known mouse position in screen coordinates (pixels) relative to the window.
    /// </summary>
    /// <returns>Mouse position in screen coordinates relative to the window.</returns>
    /// <remarks>
    ///     The returned mouse position may not be the true mouse position when the player is interacting
    ///     with the GUI. The GUI will often intercept mouse events leading to this value becoming stale.
    /// </remarks>
    public Vector2 GetMousePosition()
    {
        return mouseState.MousePosition;
    }

    /// <summary>
    ///     Gets whether the given mouse button is pressed down.
    /// </summary>
    /// <param name="button">Mouse button.</param>
    /// <returns>true if button is pressed down, false otherwise.</returns>
    public bool IsMouseButtonDown(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => mouseState.IsLeftButtonDown,
            MouseButton.Middle => mouseState.IsMiddleButtonDown,
            MouseButton.Right => mouseState.IsRightButtonDown,
            _ => false
        };
    }

    /// <summary>
    ///     Gets the cumulative change in mouse wheel position since engine startup.
    /// </summary>
    /// <returns>Wheel change (positive is scroll up, negative is scroll down).</returns>
    public float GetCumulativeMouseScroll()
    {
        return mouseState.TotalScrollAmount;
    }

    /// <summary>
    ///     Gets whether the given keyboard key is pressed down.
    /// </summary>
    /// <param name="keycode">Key.</param>
    /// <returns>true if key is pressed, false otherwise.</returns>
    public bool IsKeyDown(SDL.SDL_Keycode keycode)
    {
        return keyboardState[keycode];
    }
}