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

using System;
using System.Text;
using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas;

/// <summary>
///     Responsible for managing the texture atlas.
/// </summary>
public class TextureAtlasManager
{
    private readonly GuiFontAtlas fontAtlas;

    /// <summary>
    ///     Main display.
    /// </summary>
    private readonly MainDisplay mainDisplay;

    /// <summary>
    ///     Spritesheet manager.
    /// </summary>
    private readonly SpriteSheetManager spriteSheetManager;

    public TextureAtlasManager(MainDisplay mainDisplay, SpriteSheetManager spriteSheetManager,
        GuiFontAtlas fontAtlas)
    {
        this.mainDisplay = mainDisplay;
        this.spriteSheetManager = spriteSheetManager;
        this.fontAtlas = fontAtlas;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

    /// <summary>
    ///     Texture atlas.
    /// </summary>
    public TextureAtlas? TextureAtlas { get; private set; }

    /// <summary>
    ///     Initializes the texture atlas.
    /// </summary>
    public void InitializeTextureAtlas()
    {
        Logger.Info("Creating the texture atlas.");

        if (mainDisplay.DisplayMode == null)
        {
            var msg = "Attempted to create texture atlas without a display mode.";
            Logger.Fatal(msg);
            ErrorHandler.Error(msg);
            throw new FatalErrorException(msg);
        }

        try
        {
            TextureAtlas?.Dispose();
            TextureAtlas = new TextureAtlas(spriteSheetManager.SpriteSheets.Values,
                fontAtlas, mainDisplay.DisplayMode.DisplayFormat);
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
    ///     Releases the texture atlas.
    /// </summary>
    public void ReleaseTextureAtlas()
    {
        TextureAtlas?.Dispose();
    }
}