/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using SDL2;
using Sovereign.ClientCore.Events;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Processes keyboard events.
/// </summary>
public class KeyboardEventHandler
{
    /// <summary>
    ///     Keyboard state.
    /// </summary>
    private readonly KeyboardState keyboardState;

    /// <summary>
    ///     Player input movement mapper.
    /// </summary>
    private readonly PlayerInputMovementMapper playerInputMovementMapper;

    public KeyboardEventHandler(KeyboardState keyboardState,
        PlayerInputMovementMapper playerInputMovementMapper)
    {
        this.keyboardState = keyboardState;
        this.playerInputMovementMapper = playerInputMovementMapper;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Client_Input_KeyDown:
                HandleKeyDownEvent(ev);
                break;

            case EventId.Client_Input_KeyUp:
                HandleKeyUpEvent(ev);
                break;

            /* Ignore unhandled events. */
        }
    }

    /// <summary>
    ///     Handles key down events.
    /// </summary>
    /// <param name="ev">Key down event.</param>
    private void HandleKeyDownEvent(Event ev)
    {
        var details = (KeyEventDetails)ev.EventDetails;

        /* Update the keyboard state. */
        var oldState = keyboardState[details.Key];
        keyboardState.KeyDown(details.Key);

        /* Perform additional processing. */
        switch (details.Key)
        {
            /* Direction keys. */
            case SDL.SDL_Keycode.SDLK_UP:
            case SDL.SDL_Keycode.SDLK_DOWN:
            case SDL.SDL_Keycode.SDLK_LEFT:
            case SDL.SDL_Keycode.SDLK_RIGHT:
                HandleDirectionKeyEvent(ev, oldState, true);
                break;

            /* Ignore keys that don't do anything for now. */
        }
    }

    /// <summary>
    ///     Handles key up events.
    /// </summary>
    /// <param name="ev">Key up event.</param>
    private void HandleKeyUpEvent(Event ev)
    {
        var details = (KeyEventDetails)ev.EventDetails;

        /* Update the keyboard state. */
        var oldState = keyboardState[details.Key];
        keyboardState.KeyUp(details.Key);

        /* Perform additional processing. */
        switch (details.Key)
        {
            /* Direction keys. */
            case SDL.SDL_Keycode.SDLK_UP:
            case SDL.SDL_Keycode.SDLK_DOWN:
            case SDL.SDL_Keycode.SDLK_LEFT:
            case SDL.SDL_Keycode.SDLK_RIGHT:
                HandleDirectionKeyEvent(ev, oldState, false);
                break;

            /* Ignore keys that don't do anything for now. */
        }
    }

    /// <summary>
    ///     Handles direction key events.
    /// </summary>
    /// <param name="ev">Direction key event.</param>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleDirectionKeyEvent(Event ev, bool oldState, bool newState)
    {
        /* Only update movement if the state has changed. */
        if (oldState != newState)
            playerInputMovementMapper.UpdateMovement(keyboardState[SDL.SDL_Keycode.SDLK_UP],
                keyboardState[SDL.SDL_Keycode.SDLK_DOWN],
                keyboardState[SDL.SDL_Keycode.SDLK_LEFT],
                keyboardState[SDL.SDL_Keycode.SDLK_RIGHT]);
    }
}