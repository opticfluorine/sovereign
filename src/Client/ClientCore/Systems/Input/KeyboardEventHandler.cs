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

using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Processes keyboard events.
/// </summary>
public class KeyboardEventHandler
{
    private readonly GlobalKeyboardShortcuts globalKeyboardShortcuts;
    private readonly InGameInputHandler inGameInputHandler;

    /// <summary>
    ///     Keyboard state.
    /// </summary>
    private readonly KeyboardState keyboardState;

    private readonly ILogger<KeyboardEventHandler> logger;

    private readonly NullInputHandler nullInputHandler;

    /// <summary>
    ///     Player input movement mapper.
    /// </summary>
    private readonly PlayerInputMovementMapper playerInputMovementMapper;

    private readonly ClientStateServices stateServices;

    public KeyboardEventHandler(KeyboardState keyboardState,
        PlayerInputMovementMapper playerInputMovementMapper,
        ClientStateServices stateServices, InGameInputHandler inGameInputHandler,
        NullInputHandler nullInputHandler, GlobalKeyboardShortcuts globalKeyboardShortcuts,
        ILogger<KeyboardEventHandler> logger)
    {
        this.keyboardState = keyboardState;
        this.playerInputMovementMapper = playerInputMovementMapper;
        this.stateServices = stateServices;
        this.inGameInputHandler = inGameInputHandler;
        this.nullInputHandler = nullInputHandler;
        this.globalKeyboardShortcuts = globalKeyboardShortcuts;
        this.logger = logger;
    }

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
        if (ev.EventDetails == null)
        {
            logger.LogError("Received KeyDown without details.");
            return;
        }

        var details = (KeyEventDetails)ev.EventDetails;

        /* Update the keyboard state. */
        var oldState = keyboardState[details.Key];
        keyboardState.KeyDown(details.Key);

        globalKeyboardShortcuts.OnKeyDown(details.Key);
        DoStateSpecificProcessing(details, false, oldState);
    }

    /// <summary>
    ///     Handles key up events.
    /// </summary>
    /// <param name="ev">Key up event.</param>
    private void HandleKeyUpEvent(Event ev)
    {
        if (ev.EventDetails == null)
        {
            logger.LogError("Received KeyUp without details.");
            return;
        }

        var details = (KeyEventDetails)ev.EventDetails;

        /* Update the keyboard state. */
        var oldState = keyboardState[details.Key];
        keyboardState.KeyUp(details.Key);

        DoStateSpecificProcessing(details, true, oldState);
    }

    /// <summary>
    ///     Performs additional keyboard input processing based on the current client state.
    /// </summary>
    /// <param name="details">Keyboard event details.</param>
    /// <param name="isKeyUp">If true, treat as a key up event; key down event otherwise.</param>
    /// <param name="oldState">Previous state of the affected key (true is key down).</param>
    private void DoStateSpecificProcessing(KeyEventDetails details, bool isKeyUp, bool oldState)
    {
        IInputHandler handler = stateServices.State switch
        {
            MainClientState.MainMenu => nullInputHandler,
            MainClientState.InGame => inGameInputHandler,
            _ => nullInputHandler
        };
        handler.HandleKeyboardEvent(details, isKeyUp, oldState);
    }
}