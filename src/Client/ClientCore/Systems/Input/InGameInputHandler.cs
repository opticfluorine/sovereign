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

using SDL3;
using Sovereign.ClientCore.Events.Details;

namespace Sovereign.ClientCore.Systems.Input;

public class InGameInputHandler : IInputHandler
{
    private readonly InGameKeyboardShortcuts inGameKeyboardShortcuts;
    private readonly PlayerInteractionHandler interactionHandler;
    private readonly KeyboardState keyboardState;
    private readonly PlayerInputMovementMapper playerInputMovementMapper;

    public InGameInputHandler(KeyboardState keyboardState, PlayerInputMovementMapper playerInputMovementMapper,
        InGameKeyboardShortcuts inGameKeyboardShortcuts, PlayerInteractionHandler interactionHandler)
    {
        this.keyboardState = keyboardState;
        this.playerInputMovementMapper = playerInputMovementMapper;
        this.inGameKeyboardShortcuts = inGameKeyboardShortcuts;
        this.interactionHandler = interactionHandler;
    }

    public void HandleKeyboardEvent(KeyEventDetails details, bool isKeyUp, bool oldState)
    {
        if (!isKeyUp) inGameKeyboardShortcuts.OnKeyDown(details.Key);

        switch (details.Key)
        {
            /* Direction keys. */
            case SDL.Keycode.Up:
            case SDL.Keycode.Down:
            case SDL.Keycode.Left:
            case SDL.Keycode.Right:
            case SDL.Keycode.W:
            case SDL.Keycode.A:
            case SDL.Keycode.S:
            case SDL.Keycode.D:
                HandleDirectionKeyEvent(oldState, !isKeyUp);
                break;

            case SDL.Keycode.Space:
                HandleSpaceKeyEvent(oldState, !isKeyUp);
                break;

            case SDL.Keycode.E:
                HandleEKeyEvent(oldState, !isKeyUp);
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
                keyboardState[SDL.Keycode.Up] || keyboardState[SDL.Keycode.W],
                keyboardState[SDL.Keycode.Down] || keyboardState[SDL.Keycode.S],
                keyboardState[SDL.Keycode.Left] || keyboardState[SDL.Keycode.A],
                keyboardState[SDL.Keycode.Right] || keyboardState[SDL.Keycode.D]);
    }

    /// <summary>
    ///     Handles space key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleSpaceKeyEvent(bool oldState, bool newState)
    {
        if (!oldState && newState) playerInputMovementMapper.Jump();
    }

    /// <summary>
    ///     Handles E key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleEKeyEvent(bool oldState, bool newState)
    {
        if (oldState && !newState) interactionHandler.Interact();
    }
}