/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Events;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Events
{

    /// <summary>
    /// Converts SDL events into engine events.
    /// </summary>
    public class SDLEventAdapter : IEventAdapter
    {

        public SDLEventAdapter(EventAdapterManager adapterManager)
        {
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
            while (ev == null && SDL.SDL_PollEvent(out SDL.SDL_Event sdlEv) == 1)
            {
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

                    default:
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
            var details = new KeyEventDetails() { Key = sdlEv.key.keysym.sym };
            return new Event(EventId.Client_Input_KeyDown, details);
        }

        private Event AdaptSdlKeyUp(SDL.SDL_Event sdlEv)
        {
            var details = new KeyEventDetails() { Key = sdlEv.key.keysym.sym };
            return new Event(EventId.Client_Input_KeyUp, details);
        }

    }

}
