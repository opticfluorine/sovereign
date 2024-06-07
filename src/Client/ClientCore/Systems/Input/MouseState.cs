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
///     Tracks the state of the mouse.
/// </summary>
public class MouseState
{
    /// <summary>
    ///     Latest known mouse position in screen coordinates (pixels) relative to the window.
    ///     May not be the true mouse position if the GUI is intercepting mouse events.
    /// </summary>
    public Vector2 MousePosition { get; private set; }

    /// <summary>
    ///     Updates the mouse position.
    /// </summary>
    /// <param name="x">Mouse x position relative to window.</param>
    /// <param name="y">Mouse y position relative to window.</param>
    public void UpdateMousePosition(int x, int y)
    {
        MousePosition = new Vector2(x, y);
    }
}