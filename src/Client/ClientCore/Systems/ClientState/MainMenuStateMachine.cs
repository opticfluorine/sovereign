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

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     State machine governing the main menu.
/// </summary>
public class MainMenuStateMachine
{
    /// <summary>
    ///     Current main menu state.
    /// </summary>
    public MainMenuState State { get; private set; } = MainMenuState.Startup;

    /// <summary>
    ///     Flag indicating whether an internal GUI reset is required.
    /// </summary>
    public bool NeedReset { get; private set; }

    /// <summary>
    ///     Sets the main menu state.
    /// </summary>
    /// <param name="state">Main menu state.</param>
    public void SetState(MainMenuState state)
    {
        State = state;
        NeedReset = true;
    }

    /// <summary>
    ///     Clears the reset flag.
    /// </summary>
    public void ClearReset()
    {
        NeedReset = false;
    }
}