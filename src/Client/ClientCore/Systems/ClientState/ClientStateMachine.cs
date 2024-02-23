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
///     Top-level client state machine.
/// </summary>
public class ClientStateMachine
{
    /// <summary>
    ///     Current client state.
    /// </summary>
    public ClientState State { get; private set; } = ClientState.MainMenu;

    /// <summary>
    ///     Attempts a state transition.
    /// </summary>
    /// <param name="desiredState">Desired target state.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryTransition(ClientState desiredState)
    {
        var valid = (State, desiredState) switch
        {
            (ClientState.MainMenu, ClientState.InGame) => true,
            (ClientState.InGame, ClientState.MainMenu) => true,
            _ => false
        };
        if (valid) State = desiredState;
        return valid;
    }
}