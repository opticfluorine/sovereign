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

using Castle.Core.Logging;
using Sovereign.ClientCore.Events.Details;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Handles mouse-related input events.
/// </summary>
public class MouseEventHandler
{
    private readonly MouseState mouseState;

    public MouseEventHandler(MouseState mouseState)
    {
        this.mouseState = mouseState;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
                    Logger.Error("Received MouseMotion event without details.");
                    break;
                }

                HandleMouseMotionEvent(details);
            }
                break;
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
}