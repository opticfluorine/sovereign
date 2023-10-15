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

using SDL2;
using Sovereign.ClientCore.Rendering.Gui;
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

    public bool PollEvent(out Event ev)
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
            }
        }

        return ev != null;
    }

    private Event AdaptSdlQuit(SDL.SDL_Event sdlEv)
    {
        return new Event(EventId.Core_Quit);
    }

    private Event AdaptSdlKeyDown(SDL.SDL_Event sdlEv)
    {
        var details = new KeyEventDetails { Key = sdlEv.key.keysym.sym };
        return new Event(EventId.Client_Input_KeyDown, details);
    }

    private Event AdaptSdlKeyUp(SDL.SDL_Event sdlEv)
    {
        var details = new KeyEventDetails { Key = sdlEv.key.keysym.sym };
        return new Event(EventId.Client_Input_KeyUp, details);
    }
}