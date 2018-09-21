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

using Castle.Core.Logging;
using Sovereign.ClientCore.Logging;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for managing the texture atlas.
    /// </summary>
    public class TextureAtlasManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        /// <summary>
        /// Texture atlas.
        /// </summary>
        public TextureAtlas TextureAtlas { get; private set; }

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        /// <summary>
        /// Spritesheet manager.
        /// </summary>
        private readonly SpriteSheetManager spriteSheetManager;

        public TextureAtlasManager(MainDisplay mainDisplay, SpriteSheetManager spriteSheetManager)
        {
            this.mainDisplay = mainDisplay;
            this.spriteSheetManager = spriteSheetManager;
        }

        /// <summary>
        /// Initializes the texture atlas.
        /// </summary>
        public void InitializeTextureAtlas()
        {
            Logger.Info("Creating the texture atlas.");

            try
            {
                TextureAtlas = new TextureAtlas(spriteSheetManager.SpriteSheets.Values,
                    mainDisplay.DisplayMode.DisplayFormat);
            }
            catch (Exception e)
            {
                /* Report error. */
                var baseMessage = "Failed to create the texture atlas.";
                Logger.Fatal(baseMessage, e);

                var sb = new StringBuilder();
                sb.Append(baseMessage).Append("\n\n").Append(e.Message);
                ErrorHandler.Error(sb.ToString());

                throw new FatalErrorException(baseMessage, e);
            }

            var w = TextureAtlas.AtlasSurface.Properties.Width;
            var h = TextureAtlas.AtlasSurface.Properties.Height;
            Logger.InfoFormat("Texture atlas created ({0} x {1}).", w, h);
        }

        /// <summary>
        /// Releases the texture atlas.
        /// </summary>
        public void ReleaseTextureAtlas()
        {
            TextureAtlas.Dispose();
        }

    }

}
