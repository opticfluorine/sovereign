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
using SDL2;
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
    private readonly Dictionary<SDL.SDL_Keycode, Action> shortcutTable = new();

    private readonly ClientStateController stateController;
    private readonly ClientStateServices stateServices;

    public GlobalKeyboardShortcuts(IEventSender eventSender, ClientStateServices stateServices,
        ClientStateController stateController, Keybindings keybindings)
    {
        this.eventSender = eventSender;
        this.stateServices = stateServices;
        this.stateController = stateController;

        Register(keybindings, ClientStateFlag.ShowNetworkDebug);
        Register(keybindings, ClientStateFlag.ShowImGuiDebugLog);
        Register(keybindings, ClientStateFlag.ShowImGuiDemo);
        Register(keybindings, ClientStateFlag.ShowImGuiIdStackTool);
        Register(keybindings, ClientStateFlag.ShowImGuiMetrics);
        Register(keybindings, ClientStateFlag.DebugFrame);
        Register(keybindings, ClientStateFlag.ShowResourceEditor);
    }

    /// <summary>
    ///     Processes shortcuts when a key is pressed.
    /// </summary>
    /// <param name="key">Released key.</param>
    public void OnKeyDown(SDL.SDL_Keycode key)
    {
        if (shortcutTable.TryGetValue(key, out var action))
            action.Invoke();
    }

    /// <summary>
    ///     Registers the toggle shortcut for a state flag using its configured key.
    /// </summary>
    /// <param name="keybindings">Parsed client keyboard bindings.</param>
    /// <param name="flag">State flag to toggle.</param>
    private void Register(Keybindings keybindings, ClientStateFlag flag)
    {
        var key = keybindings.GlobalShortcutKey(flag);
        if (key == SDL.SDL_Keycode.SDLK_UNKNOWN) return;

        shortcutTable[key] = () => Toggle(flag);
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