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
using System.Numerics;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Tracks the state of the mouse.
/// </summary>
public class MouseState
{
    /// <summary>
    ///     Amount of scroll to produce a scroll tick event.
    /// </summary>
    private const float ScrollTickThreshold = 1.0f;

    private readonly IEventSender eventSender;
    private readonly InputInternalController internalController;

    /// <summary>
    ///     Scroll wheel position for the last scroll tick.
    /// </summary>
    private float lastScrollTick;

    public MouseState(InputInternalController internalController, IEventSender eventSender)
    {
        this.internalController = internalController;
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Latest known mouse position in screen coordinates (pixels) relative to the window.
    ///     May not be the true mouse position if the GUI is intercepting mouse events.
    /// </summary>
    public Vector2 MousePosition { get; private set; }

    /// <summary>
    ///     If true, indicates the left mouse button is pressed down.
    /// </summary>
    public bool IsLeftButtonDown { get; private set; }

    /// <summary>
    ///     If true, indicates the middle mouse button is pressed down.
    /// </summary>
    public bool IsMiddleButtonDown { get; private set; }

    /// <summary>
    ///     If true, indicates the right mouse button is pressed down.
    /// </summary>
    public bool IsRightButtonDown { get; private set; }

    /// <summary>
    ///     Cumulative amount the mouse wheel has scrolled since start. Positive is scroll up, negative
    ///     is scroll down.
    /// </summary>
    public float TotalScrollAmount { get; private set; }

    /// <summary>
    ///     Updates the mouse position.
    /// </summary>
    /// <param name="x">Mouse x position relative to window.</param>
    /// <param name="y">Mouse y position relative to window.</param>
    public void UpdateMousePosition(int x, int y)
    {
        MousePosition = new Vector2(x, y);
    }

    /// <summary>
    ///     Updates mouse button state.
    /// </summary>
    /// <param name="button">Button.</param>
    /// <param name="isDown">true if mouse button is down, false if mouse button is up.</param>
    public void SetButtonState(MouseButton button, bool isDown)
    {
        switch (button)
        {
            case MouseButton.Left:
                IsLeftButtonDown = isDown;
                break;

            case MouseButton.Middle:
                IsMiddleButtonDown = isDown;
                break;

            case MouseButton.Right:
                IsRightButtonDown = isDown;
                break;
        }
    }

    /// <summary>
    ///     Updates the scroll wheel cumulative scroll.
    /// </summary>
    /// <param name="amount">Relative amount scrolled (positive is up, negative is down).</param>
    public void UpdateWheel(float amount)
    {
        TotalScrollAmount += amount;

        // Check if tick threshold exceeded in either direction.
        var delta = TotalScrollAmount - lastScrollTick;
        while (Math.Abs(delta) > ScrollTickThreshold)
        {
            internalController.AnnounceScrollTick(eventSender, delta > 0.0f);

            var step = Math.Sign(delta) * ScrollTickThreshold;
            lastScrollTick += step;
            delta -= step;
        }
    }
}