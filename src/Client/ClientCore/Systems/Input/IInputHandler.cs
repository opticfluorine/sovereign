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

using Sovereign.ClientCore.Events.Details;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Interface for input handlers.
/// </summary>
public interface IInputHandler
{
    /// <summary>
    ///     Handles an input event.
    /// </summary>
    /// <param name="details">Keyboard event details.</param>
    /// <param name="isKeyUp">If true, key up event; key down otherwise.</param>
    /// <param name="oldState">Previous state of the affected key.</param>
    void HandleKeyboardEvent(KeyEventDetails details, bool isKeyUp, bool oldState);
}