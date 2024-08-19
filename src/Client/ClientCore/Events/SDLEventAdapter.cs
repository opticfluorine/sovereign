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

using System.Diagnostics.CodeAnalysis;
using SDL2;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Events;

/// <summary>
///     Converts SDL events into engine events.
/// </summary>
public class SDLEventAdapter : IEventAdapter
{
    private readonly CommonGuiManager guiManager;

    public SDLEventAdapter(EventAdapterManager adapterManager, CommonGuiManager guiManager)
    {
        this.guiManager = guiManager;

        adapterManager.RegisterEventAdapter(this);
    }

    public void PrepareEvents()
    {
        /* No preparation is needed with SDL. */
    }

    public bool PollEvent([NotNullWhen(true)] out Event? ev)
    {
        /*
         * Attempt to adapt the next available event until successful
         * or no events remain.
         */
        ev = null;
        while (ev == null && SDL.SDL_PollEvent(out var sdlEv) == 1)
        {
            /*
             * Occasionally the GUI system will entirely consume keyboard
             * and mouse events. When this occurs, we do not convert the
             * SDL event to the corresponding internal event.
             */
            guiManager.ProcessEvent(ref sdlEv, out var shouldDispatch);
            if (!shouldDispatch) continue;

            switch (sdlEv.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    ev = AdaptSdlQuit(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_KEYDOWN:
                    ev = AdaptSdlKeyDown(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    ev = AdaptSdlKeyUp(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    ev = AdaptSdlMouseMotion(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    ev = AdaptSdlMouseDown(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    ev = AdaptSdlMouseUp(sdlEv);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    ev = AdaptSdlMouseWheel(sdlEv);
                    break;
            }
        }

        return ev != null;
    }

    /// <summary>
    ///     Adapts an SDL_QUIT event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlQuit(SDL.SDL_Event sdlEv)
    {
        return new Event(EventId.Core_Quit);
    }

    /// <summary>
    ///     Adapts an SDL_KEYDOWN event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlKeyDown(SDL.SDL_Event sdlEv)
    {
        var details = new KeyEventDetails { Key = sdlEv.key.keysym.sym };
        return new Event(EventId.Client_Input_KeyDown, details);
    }

    /// <summary>
    ///     Adapts an SDL_KEYUP event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlKeyUp(SDL.SDL_Event sdlEv)
    {
        var details = new KeyEventDetails { Key = sdlEv.key.keysym.sym };
        return new Event(EventId.Client_Input_KeyUp, details);
    }

    /// <summary>
    ///     Adapts an SDL_MOUSEMOTION event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlMouseMotion(SDL.SDL_Event sdlEv)
    {
        var details = new MouseMotionEventDetails
        {
            X = sdlEv.motion.x,
            Y = sdlEv.motion.y
        };
        return new Event(EventId.Client_Input_MouseMotion, details);
    }

    /// <summary>
    ///     Adapts a SDL_MOUSEBUTTONDOWN event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlMouseDown(SDL.SDL_Event sdlEv)
    {
        var details = new MouseButtonEventDetails
        {
            Button = MapMouseButton(sdlEv.button.button)
        };
        return new Event(EventId.Client_Input_MouseDown, details);
    }

    /// <summary>
    ///     Adapts a SDL_MOUSEBUTTONUP event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlMouseUp(SDL.SDL_Event sdlEv)
    {
        var details = new MouseButtonEventDetails
        {
            Button = MapMouseButton(sdlEv.button.button)
        };
        return new Event(EventId.Client_Input_MouseUp, details);
    }

    /// <summary>
    ///     Adapts a SDL_MOUSEWHEEL event.
    /// </summary>
    /// <param name="sdlEv">SDL event.</param>
    /// <returns>Internal event.</returns>
    private Event AdaptSdlMouseWheel(SDL.SDL_Event sdlEv)
    {
        var details = new MouseWheelEventDetails
        {
            ScrollAmount = sdlEv.wheel.preciseY
        };
        return new Event(EventId.Client_Input_MouseWheel, details);
    }

    /// <summary>
    ///     Maps a SDL mouse button enum to the internal MouseButton enum.
    /// </summary>
    /// <param name="button">SDL button.</param>
    /// <returns>Corresponding MouseButton value.</returns>
    private MouseButton MapMouseButton(uint button)
    {
        return button switch
        {
            SDL.SDL_BUTTON_LEFT => MouseButton.Left,
            SDL.SDL_BUTTON_MIDDLE => MouseButton.Middle,
            SDL.SDL_BUTTON_RIGHT => MouseButton.Right,
            _ => MouseButton.Other
        };
    }
}