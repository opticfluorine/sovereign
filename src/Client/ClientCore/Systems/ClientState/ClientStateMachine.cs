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
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Top-level client state machine.
/// </summary>
public class ClientStateMachine
{
    private readonly ClientStateFlagManager flagManager;

    private readonly Dictionary<MainClientState, Action> stateEntryHandlers;
    private readonly Dictionary<MainClientState, Action> stateExitHandlers;

    public ClientStateMachine(IOptions<AutoUpdaterOptions> autoUpdaterOptions, ClientStateFlagManager flagManager)
    {
        this.flagManager = flagManager;

        stateEntryHandlers = new Dictionary<MainClientState, Action>
        {
            { MainClientState.InGame, OnEnterInGame }
        };

        stateExitHandlers = new Dictionary<MainClientState, Action>
        {
            { MainClientState.Update, OnExitUpdate }
        };

        State = autoUpdaterOptions.Value.UpdateOnStartup
            ? MainClientState.Update
            : MainClientState.MainMenu;
    }

    /// <summary>
    ///     Current client state.
    /// </summary>
    public MainClientState State { get; private set; }

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

            if (stateExitHandlers.TryGetValue(priorState, out var handler)) handler.Invoke();
            if (stateEntryHandlers.TryGetValue(desiredState, out handler)) handler.Invoke();

            State = desiredState;
        }

        return valid;
    }

    /// <summary>
    ///     Called when about to enter the InGame state.
    /// </summary>
    private void OnEnterInGame()
    {
        flagManager.SetStateFlagValue(ClientStateFlag.ShowChat, true);
        flagManager.SetStateFlagValue(ClientStateFlag.NewLogin, true);
    }

    /// <summary>
    ///     Called when the Update state is about to exit.
    /// </summary>
    private void OnExitUpdate()
    {
        flagManager.SetStateFlagValue(ClientStateFlag.ReloadClientResources, true);
    }
}