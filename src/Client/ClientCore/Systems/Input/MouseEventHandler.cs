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
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Handles mouse-related input events.
/// </summary>
public class MouseEventHandler(
    MouseState mouseState,
    ILogger<MouseEventHandler> logger,
    ClientStateServices stateServices,
    InGameInputHandler inGameHandler,
    NullInputHandler nullHandler)
{
    /// <summary>
    ///     Handles a mouse-related event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void HandleMouseEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Client_Input_MouseMotion:
            {
                if (ev.EventDetails is not MouseMotionEventDetails details)
                {
                    logger.LogError("Received MouseMotion event without details.");
                    break;
                }

                HandleMouseMotionEvent(details);
                break;
            }

            case EventId.Client_Input_MouseUp:
            {
                if (ev.EventDetails is not MouseButtonEventDetails details)
                {
                    logger.LogError("Received MouseUp event without details.");
                    break;
                }

                HandleMouseButtonUpEvent(details);
                break;
            }

            case EventId.Client_Input_MouseDown:
            {
                if (ev.EventDetails is not MouseButtonEventDetails details)
                {
                    logger.LogError("Received MouseDown event without details.");
                    break;
                }

                HandleMouseButtonDownEvent(details);
                break;
            }

            case EventId.Client_Input_MouseWheel:
            {
                if (ev.EventDetails is not MouseWheelEventDetails details)
                {
                    logger.LogError("Received MouseWheel event without details.");
                    break;
                }

                HandleMouseWheelEvent(details);
                break;
            }
        }
    }

    /// <summary>
    ///     Handles a mouse motion event.
    /// </summary>
    /// <param name="details">Mouse motion event.</param>
    private void HandleMouseMotionEvent(MouseMotionEventDetails details)
    {
        mouseState.UpdateMousePosition(details.X, details.Y);
    }

    /// <summary>
    ///     Handles a mouse button down event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleMouseButtonDownEvent(MouseButtonEventDetails details)
    {
        mouseState.SetButtonState(details.Button, true);
        DoButtonStateSpecificProcessing(details, true);
    }

    /// <summary>
    ///     Handles a mouse button up event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleMouseButtonUpEvent(MouseButtonEventDetails details)
    {
        mouseState.SetButtonState(details.Button, false);
        DoButtonStateSpecificProcessing(details, false);
    }

    /// <summary>
    ///     Handles a mouse wheel event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleMouseWheelEvent(MouseWheelEventDetails details)
    {
        mouseState.UpdateWheel(details.ScrollAmount);
    }

    /// <summary>
    ///     Dispatches state-specific mouse button event processing.
    /// </summary>
    /// <param name="details">Event details.</param>
    /// <param name="isDown">Whether the mouse button is down.</param>
    private void DoButtonStateSpecificProcessing(MouseButtonEventDetails details, bool isDown)
    {
        IInputHandler handler = stateServices.State switch
        {
            MainClientState.InGame => inGameHandler,
            _ => nullHandler
        };

        handler.HandleMouseButtonEvent(details, isDown);
    }
}