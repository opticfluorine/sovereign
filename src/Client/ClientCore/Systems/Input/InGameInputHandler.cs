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
    ClientStateController stateController,
    Keybindings keybindings)
    : IInputHandler
{
    /// <summary>
    ///     Movement directions that can be bound to keys.
    /// </summary>
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    ///     Map from keycode to movement direction.
    /// </summary>
    private readonly Dictionary<SDL.SDL_Keycode, Direction> movementKeys = BuildMovementKeys(keybindings);

    /// <summary>
    ///     Map from keycode to hotbar slot index.
    /// </summary>
    private readonly Dictionary<SDL.SDL_Keycode, int> hotbarKeys = BuildHotbarKeys(keybindings);

    public void HandleKeyboardEvent(KeyEventDetails details, bool isKeyUp, bool oldState)
    {
        if (!isKeyUp) inGameKeyboardShortcuts.OnKeyDown(details.Key);

        var newState = !isKeyUp;
        if (movementKeys.ContainsKey(details.Key))
        {
            HandleDirectionKeyEvent(oldState, newState);
            return;
        }

        if (details.Key == keybindings.Jump)
        {
            HandleJumpKeyEvent(oldState, newState);
            return;
        }

        if (details.Key == keybindings.Interact)
        {
            HandleInteractKeyEvent(oldState, newState);
            return;
        }

        if (details.Key == keybindings.PickUpItem)
        {
            HandlePickUpItemKeyEvent(oldState, newState);
            return;
        }

        if (hotbarKeys.TryGetValue(details.Key, out var slotIndex))
            HandleNumberKeyEvent(slotIndex, oldState, newState);

        /* Ignore keys that don't do anything for now. */
    }

    /// <summary>
    ///     Handles mouse button events.
    /// </summary>
    /// <param name="details">Event details.</param>
    /// <param name="isButtonDown">true if mouse button is down, false otherwise.</param>
    public void HandleMouseButtonEvent(MouseButtonEventDetails details, bool isButtonDown)
    {
        if (!isButtonDown) return;

        if (stateServices.TryGetSelectedInventorySlot(out var slotIndex, out var selectedQty))
        {
            inventoryClickHandler.DropSelectedItem(slotIndex, selectedQty,
                keyboardState.IsAnyDown(keybindings.DropItemModifier));
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
    ///     Builds the movement key lookup from the configured movement bindings.
    /// </summary>
    /// <param name="keybindings">Parsed client keyboard bindings.</param>
    /// <returns>Map from keycode to movement direction.</returns>
    private static Dictionary<SDL.SDL_Keycode, Direction> BuildMovementKeys(Keybindings keybindings)
    {
        var keys = new Dictionary<SDL.SDL_Keycode, Direction>();
        AddMovementKeys(keys, keybindings.MoveUp, Direction.Up);
        AddMovementKeys(keys, keybindings.MoveDown, Direction.Down);
        AddMovementKeys(keys, keybindings.MoveLeft, Direction.Left);
        AddMovementKeys(keys, keybindings.MoveRight, Direction.Right);
        return keys;
    }

    /// <summary>
    ///     Adds the keys for one movement direction to the movement key lookup.
    /// </summary>
    /// <param name="keys">Movement key lookup.</param>
    /// <param name="bindings">Bound keys for the direction.</param>
    /// <param name="direction">Movement direction.</param>
    private static void AddMovementKeys(Dictionary<SDL.SDL_Keycode, Direction> keys,
        IReadOnlyList<SDL.SDL_Keycode> bindings, Direction direction)
    {
        foreach (var key in bindings)
            if (key != SDL.SDL_Keycode.SDLK_UNKNOWN) keys[key] = direction;
    }

    /// <summary>
    ///     Builds the hotbar key lookup from the configured hotbar slot bindings.
    /// </summary>
    /// <param name="keybindings">Parsed client keyboard bindings.</param>
    /// <returns>Map from keycode to hotbar slot index.</returns>
    private static Dictionary<SDL.SDL_Keycode, int> BuildHotbarKeys(Keybindings keybindings)
    {
        var keys = new Dictionary<SDL.SDL_Keycode, int>();
        var slotKeys = keybindings.HotbarSlotKeys;
        for (var i = 0; i < slotKeys.Count; ++i)
        {
            var key = slotKeys[i];
            if (key != SDL.SDL_Keycode.SDLK_UNKNOWN) keys[key] = i;
        }

        return keys;
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
                keyboardState.IsAnyDown(keybindings.MoveUp),
                keyboardState.IsAnyDown(keybindings.MoveDown),
                keyboardState.IsAnyDown(keybindings.MoveLeft),
                keyboardState.IsAnyDown(keybindings.MoveRight));
    }

    /// <summary>
    ///     Handles jump key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleJumpKeyEvent(bool oldState, bool newState)
    {
        if (!oldState && newState) playerInputMovementMapper.Jump();
    }

    /// <summary>
    ///     Handles interact key events.
    /// </summary>
    /// <param name="oldState">Old state of the key.</param>
    /// <param name="newState">New state of the key.</param>
    private void HandleInteractKeyEvent(bool oldState, bool newState)
    {
        if (oldState && !newState) playerController.Interact(eventSender);
    }

    /// <summary>
    ///     Handles pick up item key events.
    /// </summary>
    /// <param name="oldState">Old state of the key (true = key down).</param>
    /// <param name="newState">New state of the key (true = key down).</param>
    private void HandlePickUpItemKeyEvent(bool oldState, bool newState)
    {
        if (!oldState && newState) playerController.PickUpItemUnder(eventSender);
    }

    /// <summary>
    ///     Handles hotbar slot key events.
    /// </summary>
    /// <param name="slotIndex">Hotbar slot index bound to the key.</param>
    /// <param name="oldState">Old state.</param>
    /// <param name="newState">New state.</param>
    private void HandleNumberKeyEvent(int slotIndex, bool oldState, bool newState)
    {
        if (!oldState && newState) stateController.SelectHotbar(eventSender, slotIndex);
    }
}
