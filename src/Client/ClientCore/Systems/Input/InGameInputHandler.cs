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

using Microsoft.Extensions.Logging;
using SDL2;
using Sovereign.ClientCore.Events.Details;

namespace Sovereign.ClientCore.Systems.Input;

public class InGameInputHandler : IInputHandler
{
    private readonly InGameKeyboardShortcuts inGameKeyboardShortcuts;
    private readonly KeyboardState keyboardState;
    private readonly ILogger<InGameInputHandler> logger;
    private readonly PlayerInputMovementMapper playerInputMovementMapper;

    public InGameInputHandler(KeyboardState keyboardState, PlayerInputMovementMapper playerInputMovementMapper,
        InGameKeyboardShortcuts inGameKeyboardShortcuts, ILogger<InGameInputHandler> logger)
    {
        this.keyboardState = keyboardState;
        this.playerInputMovementMapper = playerInputMovementMapper;
        this.inGameKeyboardShortcuts = inGameKeyboardShortcuts;
        this.logger = logger;
    }

    public void HandleKeyboardEvent(KeyEventDetails details, bool isKeyUp, bool oldState)
    {
        if (!isKeyUp) inGameKeyboardShortcuts.OnKeyDown(details.Key);

        switch (details.Key)
        {
            /* Direction keys. */
            case SDL.SDL_Keycode.SDLK_UP:
            case SDL.SDL_Keycode.SDLK_DOWN:
            case SDL.SDL_Keycode.SDLK_LEFT:
            case SDL.SDL_Keycode.SDLK_RIGHT:
            case SDL.SDL_Keycode.SDLK_w:
            case SDL.SDL_Keycode.SDLK_a:
            case SDL.SDL_Keycode.SDLK_s:
            case SDL.SDL_Keycode.SDLK_d:
                HandleDirectionKeyEvent(oldState, !isKeyUp);
                break;
            
            case SDL.SDL_Keycode.SDLK_SPACE:
                HandleSpaceKeyEvent(oldState, !isKeyUp);
                break;

            /* Ignore keys that don't do anything for now. */
        }
    }

    /// <summary>
    ///     Handles direction key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleDirectionKeyEvent(bool oldState, bool newState)
    {
        /* Only update movement if the state has changed. */
        if (oldState != newState)
            playerInputMovementMapper.UpdateMovement(
                keyboardState[SDL.SDL_Keycode.SDLK_UP] || keyboardState[SDL.SDL_Keycode.SDLK_w],
                keyboardState[SDL.SDL_Keycode.SDLK_DOWN] || keyboardState[SDL.SDL_Keycode.SDLK_s],
                keyboardState[SDL.SDL_Keycode.SDLK_LEFT] || keyboardState[SDL.SDL_Keycode.SDLK_a],
                keyboardState[SDL.SDL_Keycode.SDLK_RIGHT] || keyboardState[SDL.SDL_Keycode.SDLK_d]);
    }
    
    /// <summary>
    ///     Handles space key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleSpaceKeyEvent(bool oldState, bool newState)
    {
        if (!oldState && newState)
        {
            playerInputMovementMapper.Jump();
        }
    }
}