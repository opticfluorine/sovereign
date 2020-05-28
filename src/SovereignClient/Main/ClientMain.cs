/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
