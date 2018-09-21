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

using Sovereign.ClientCore.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using SDL2;

namespace Sovereign.SovereignClient.Main
{

    public class ClientMain
    {

        /// <summary>
        /// Main entry point.
        /// </summary>
        static void Main()
        {
            /* Initialize SDL. */
            int err = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            if (err < 0)
            {
                /* Fatal error. */
                new ErrorHandler().Error(SDL.SDL_GetError());
                return;
            }

            err = SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
            if (err < 0)
            {
                /* Fatal error. */
                new ErrorHandler().Error(SDL.SDL_GetError());
                return;
            }

            /* Run the engine. */
            CoreMain coreMain = new CoreMain();
            coreMain.RunEngine();

            /* Shut down SDL. */
            SDL_image.IMG_Quit();
            SDL.SDL_Quit();
        }
    }

}
