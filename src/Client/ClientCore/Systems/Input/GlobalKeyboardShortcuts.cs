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
using SDL3;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Handles keyboard shortcuts that apply to all client states.
/// </summary>
public class GlobalKeyboardShortcuts
{
    private readonly IEventSender eventSender;

    /// <summary>
    ///     Map from keycodes to shortcut actions.
    /// </summary>
    private readonly Dictionary<SDL.Keycode, Action> shortcutTable = new();

    private readonly ClientStateController stateController;
    private readonly ClientStateServices stateServices;

    public GlobalKeyboardShortcuts(IEventSender eventSender, ClientStateServices stateServices,
        ClientStateController stateController)
    {
        this.eventSender = eventSender;
        this.stateServices = stateServices;
        this.stateController = stateController;

        // For now the shortcuts are hardcoded.
        shortcutTable[SDL.Keycode.F7] = () => Toggle(ClientStateFlag.ShowNetworkDebug);
        shortcutTable[SDL.Keycode.F8] = () => Toggle(ClientStateFlag.ShowImGuiDebugLog);
        shortcutTable[SDL.Keycode.F9] = () => Toggle(ClientStateFlag.ShowImGuiDemo);
        shortcutTable[SDL.Keycode.F10] = () => Toggle(ClientStateFlag.ShowImGuiIdStackTool);
        shortcutTable[SDL.Keycode.F11] = () => Toggle(ClientStateFlag.ShowImGuiMetrics);
        shortcutTable[SDL.Keycode.F12] = () => Toggle(ClientStateFlag.DebugFrame);
        shortcutTable[SDL.Keycode.Grave] = () => Toggle(ClientStateFlag.ShowResourceEditor);
    }

    /// <summary>
    ///     Processes shortcuts when a key is pressed.
    /// </summary>
    /// <param name="key">Released key.</param>
    public void OnKeyDown(SDL.Keycode key)
    {
        if (shortcutTable.TryGetValue(key, out var action))
            action.Invoke();
    }

    /// <summary>
    ///     Toggles a state flag.
    /// </summary>
    /// <param name="flag">State flag.</param>
    private void Toggle(ClientStateFlag flag)
    {
        var newState = !stateServices.GetStateFlagValue(flag);
        stateController.SetStateFlag(eventSender, flag, newState);
    }
}