/*
 * Engine8 Dynamic World MMORPG Engine
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

using Castle.Core.Logging;
using Engine8.ClientCore.Events;
using Engine8.EngineCore.Events;

namespace Engine8.ClientCore.Systems.Input
{

    /// <summary>
    /// Processes keyboard events.
    /// </summary>
    public class KeyboardEventHandler
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Keyboard state.
        /// </summary>
        private readonly KeyboardState keyboardState;

        /// <summary>
        /// Player input movement mapper.
        /// </summary>
        private readonly PlayerInputMovementMapper playerInputMovementMapper;

        public KeyboardEventHandler(KeyboardState keyboardState, 
            PlayerInputMovementMapper playerInputMovementMapper)
        {
            this.keyboardState = keyboardState;
            this.playerInputMovementMapper = playerInputMovementMapper;
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
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles key down events.
        /// </summary>
        /// <param name="ev">Key down event.</param>
        private void HandleKeyDownEvent(Event ev)
        {
            var details = (KeyEventDetails)ev.EventDetails;

            /* Update the keyboard state. */
            var oldState = keyboardState[details.Key];
            keyboardState.KeyDown(details.Key);

            /* Perform additional processing. */
            switch (details.Key)
            {
                /* Direction keys. */
                case SDL2.SDL.SDL_Keycode.SDLK_UP:
                case SDL2.SDL.SDL_Keycode.SDLK_DOWN:
                case SDL2.SDL.SDL_Keycode.SDLK_LEFT:
                case SDL2.SDL.SDL_Keycode.SDLK_RIGHT:
                    HandleDirectionKeyEvent(ev, oldState, true);
                    break;

                /* Ignore keys that don't do anything for now. */
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles key up events.
        /// </summary>
        /// <param name="ev">Key up event.</param>
        private void HandleKeyUpEvent(Event ev)
        {
            var details = (KeyEventDetails)ev.EventDetails;

            /* Update the keyboard state. */
            var oldState = keyboardState[details.Key];
            keyboardState.KeyUp(details.Key);

            /* Perform additional processing. */
            switch (details.Key)
            {
                /* Direction keys. */
                case SDL2.SDL.SDL_Keycode.SDLK_UP:
                case SDL2.SDL.SDL_Keycode.SDLK_DOWN:
                case SDL2.SDL.SDL_Keycode.SDLK_LEFT:
                case SDL2.SDL.SDL_Keycode.SDLK_RIGHT:
                    HandleDirectionKeyEvent(ev, oldState, false);
                    break;

                /* Ignore keys that don't do anything for now. */
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles direction key events.
        /// </summary>
        /// <param name="ev">Direction key event.</param>
        /// <param name="oldState">Old state of the key.</param>
        /// <param name="newState">New state of the key.</param>
        private void HandleDirectionKeyEvent(Event ev, bool oldState, bool newState)
        {
            /* Only update movement if the state has changed. */
            if (oldState != newState)
            {
                playerInputMovementMapper.UpdateMovement(keyboardState[SDL2.SDL.SDL_Keycode.SDLK_UP],
                    keyboardState[SDL2.SDL.SDL_Keycode.SDLK_DOWN], 
                    keyboardState[SDL2.SDL.SDL_Keycode.SDLK_LEFT],
                    keyboardState[SDL2.SDL.SDL_Keycode.SDLK_RIGHT]);
            }
        }

    }

}
