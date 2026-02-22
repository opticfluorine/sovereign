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
using Sovereign.EngineCore.Player;

namespace Sovereign.ClientCore.Systems.Input;

public class InGameKeyboardShortcuts
{
    private readonly IEventSender eventSender;

    /// <summary>
    ///     Map from keycodes to shortcut actions.
    /// </summary>
    private readonly Dictionary<SDL.SDL_Keycode, Action> shortcutTable = new();

    private readonly ClientStateController stateController;
    private readonly ClientStateServices stateServices;

    public InGameKeyboardShortcuts(IEventSender eventSender, ClientStateServices stateServices,
        ClientStateController stateController, PlayerRoleCheck roleCheck)
    {
        this.eventSender = eventSender;
        this.stateServices = stateServices;
        this.stateController = stateController;

        // For now the shortcuts are hardcoded.
        shortcutTable[SDL.SDL_Keycode.SDLK_i] = () => Toggle(ClientStateFlag.ShowInventory);
        shortcutTable[SDL.SDL_Keycode.SDLK_RETURN] = () => Toggle(ClientStateFlag.ShowChat);
        shortcutTable[SDL.SDL_Keycode.SDLK_ESCAPE] = () => Toggle(ClientStateFlag.ShowInGameMenu);
        shortcutTable[SDL.SDL_Keycode.SDLK_F2] = () => Toggle(ClientStateFlag.ShowPlayerDebug);
        shortcutTable[SDL.SDL_Keycode.SDLK_F3] = () => Toggle(ClientStateFlag.ShowEntityDebug);
        shortcutTable[SDL.SDL_Keycode.SDLK_INSERT] = () => Toggle(ClientStateFlag.ShowTemplateEntityEditor);
        shortcutTable[SDL.SDL_Keycode.SDLK_DELETE] = () =>
        {
            if (stateServices.TryGetSelectedPlayer(out var playerId) && roleCheck.IsPlayerAdmin(playerId))
            {
                Toggle(ClientStateFlag.WorldEditMode);
                Toggle(ClientStateFlag.ShowHiddenEntities);
            }
        };
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
    ///     Toggles a state flag.
    /// </summary>
    /// <param name="flag">Flag to toggle.</param>
    private void Toggle(ClientStateFlag flag)
    {
        var newState = !stateServices.GetStateFlagValue(flag);
        stateController.SetStateFlag(eventSender, flag, newState);
    }
}