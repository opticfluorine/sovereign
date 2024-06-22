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

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Public read API for the Input system.
/// </summary>
public class InputServices
{
    private readonly MouseState mouseState;

    public InputServices(MouseState mouseState)
    {
        this.mouseState = mouseState;
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
}