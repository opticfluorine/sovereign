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

using System;
using System.Collections.Generic;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Top-level client state machine.
/// </summary>
public class ClientStateMachine
{
    private readonly RenderingManager renderingManager;

    /// <summary>
    ///     Current client state.
    /// </summary>
    public MainClientState State { get; private set; }

    private readonly Dictionary<MainClientState, Action> stateExitHandlers;

    public ClientStateMachine(RenderingManager renderingManager, ClientConfigurationManager configManager)
    {
        this.renderingManager = renderingManager;
        stateExitHandlers = new Dictionary<MainClientState, Action>()
        {
            { MainClientState.Update, OnExitUpdate }
        };

        State = configManager.ClientConfiguration.AutoUpdater.UpdateOnStartup
            ? MainClientState.Update
            : MainClientState.MainMenu;
    }

    /// <summary>
    ///     Attempts a state transition.
    /// </summary>
    /// <param name="desiredState">Desired target state.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool TryTransition(MainClientState desiredState)
    {
        var valid = (State, desiredState) switch
        {
            (MainClientState.Update, MainClientState.MainMenu) => true,
            (MainClientState.MainMenu, MainClientState.InGame) => true,
            (MainClientState.InGame, MainClientState.MainMenu) => true,
            _ => false
        };
        if (valid)
        {
            var priorState = State;

            if (stateExitHandlers.TryGetValue(priorState, out var handler))
                handler.Invoke();
            
            State = desiredState;
        }
        return valid;
    }

    /// <summary>
    ///     Called when the Update state is about to exit.
    /// </summary>
    private void OnExitUpdate()
    {
        renderingManager.RequestResourceReload();
    }
}