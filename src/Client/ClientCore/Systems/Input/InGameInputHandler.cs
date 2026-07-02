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
using SDL2;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Inventory;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.ClientCore.Systems.Player;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

public class InGameInputHandler(
    KeyboardState keyboardState,
    PlayerInputMovementMapper playerInputMovementMapper,
    InGameKeyboardShortcuts inGameKeyboardShortcuts,
    PlayerController playerController,
    IEventSender eventSender,
    IPerspectiveServices perspectiveServices,
    CameraServices cameraServices,
    EntityClickHandler entityClickHandler,
    ClientStateServices stateServices,
    InventoryClickHandler inventoryClickHandler,
    ClientStateController stateController)
    : IInputHandler
{
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

            case SDL.SDL_Keycode.SDLK_e:
                HandleEKeyEvent(oldState, !isKeyUp);
                break;

            case SDL.SDL_Keycode.SDLK_COMMA:
                HandleCommaKeyEvent(oldState, !isKeyUp);
                break;

            case SDL.SDL_Keycode.SDLK_0:
            case SDL.SDL_Keycode.SDLK_1:
            case SDL.SDL_Keycode.SDLK_2:
            case SDL.SDL_Keycode.SDLK_3:
            case SDL.SDL_Keycode.SDLK_4:
            case SDL.SDL_Keycode.SDLK_5:
            case SDL.SDL_Keycode.SDLK_6:
            case SDL.SDL_Keycode.SDLK_7:
            case SDL.SDL_Keycode.SDLK_8:
            case SDL.SDL_Keycode.SDLK_9:
            {
                var number = (int)details.Key - (int)SDL.SDL_Keycode.SDLK_0;
                HandleNumberKeyEvent(number, oldState, !isKeyUp);
                break;
            }

            /* Ignore keys that don't do anything for now. */
        }
    }

    /// <summary>
    ///     Handles mouse button events.
    /// </summary>
    /// <param name="details">Event details.</param>
    /// <param name="isButtonDown">true if mouse button is down, false otherwise.</param>
    public void HandleMouseButtonEvent(MouseButtonEventDetails details, bool isButtonDown)
    {
        if (!isButtonDown) return;

        if (stateServices.TryGetSelectedInventorySlot(out var slotIndex))
        {
            inventoryClickHandler.DropSelectedItem(slotIndex, keyboardState[SDL.SDL_Keycode.SDLK_LCTRL]);
            stateController.DeselectItem(eventSender);
        }

        // What did the player just click?
        var clickPos = cameraServices.GetMousePositionWorldCoordinates();
        if (perspectiveServices.TryGetHighestCoveringEntity(clickPos, out var entityId))
            entityClickHandler.OnEntityClicked(entityId, details.Button);
    }

    public void HandleMouseWheelEvent(MouseWheelEventDetails details)
    {
        // World edit inputs are handled by their own system, so ignore.
        if (stateServices.GetStateFlagValue(ClientStateFlag.WorldEditMode)) return;

        // Hotbar scroll.
        var delta = -Math.Sign(details.ScrollAmount);
        var newSlotIndex = (stateServices.GetSelectedHotbarSlot() + ClientInventoryConstants.HotbarSlotCount + delta) %
                           ClientInventoryConstants.HotbarSlotCount;
        stateController.SelectHotbar(eventSender, newSlotIndex);
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
        if (!oldState && newState) playerInputMovementMapper.Jump();
    }

    /// <summary>
    ///     Handles E key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleEKeyEvent(bool oldState, bool newState)
    {
        if (oldState && !newState) playerController.Interact(eventSender);
    }

    /// <summary>
    ///     Handles ',' key events.
    /// </summary>
    /// <param name="oldState">Old state of the key (true = key down).</param>
    /// <param name="newState">New state of the key (true = key down).</param>
    private void HandleCommaKeyEvent(bool oldState, bool newState)
    {
        if (!oldState && newState) playerController.PickUpItemUnder(eventSender);
    }

    /// <summary>
    ///     Handles number key events.
    /// </summary>
    /// <param name="number">Number pressed.</param>
    /// <param name="oldState">Old state.</param>
    /// <param name="newState">New state.</param>
    private void HandleNumberKeyEvent(int number, bool oldState, bool newState)
    {
        // This will dispatch to a hotbar slot selection. Slots are numbered in QWERTY layout order (1,2,3,...,0)
        // but indexed from 0 (i.e. slot 0 is key 1, slot 1 is key 2, ..., slot 9 is key 0).
        var slotIndex = (number + ClientInventoryConstants.HotbarSlotCount - 1) %
                        ClientInventoryConstants.HotbarSlotCount;
        if (!oldState && newState) stateController.SelectHotbar(eventSender, slotIndex);
    }
}