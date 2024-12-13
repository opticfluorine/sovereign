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

using Sovereign.ClientCore.Network;

namespace Sovereign.ClientCore.Configuration;

/// <summary>
///     Client configuration.
/// </summary>
/// <remarks>
///     In the future, the values of IClientConfiguration will be loaded at
///     runtime from a file.
/// </remarks>
public sealed class ClientConfiguration
{
    /// <summary>
    ///     Width of a tile in pixels.
    /// </summary>
    public int TileWidth { get; set; } = 32;

    /// <summary>
    ///     Extra tiles to search for renderable entities along the x axis.
    /// </summary>
    public float RenderSearchSpacerX { get; set; } = 4.0f;

    /// <summary>
    ///     Extra tiles to search for renderable entities along the y axis.
    /// </summary>
    public float RenderSearchSpacerY { get; set; } = 8.0f;

    /// <summary>
    ///     Connection parameters.
    /// </summary>
    public ClientConnectionParameters ConnectionParameters { get; set; } = new();

    /// <summary>
    ///     Display settings.
    /// </summary>
    public DisplayConfiguration Display { get; set; } = new();

    /// <summary>
    ///     Autoupdater settings.
    /// </summary>
    public AutoUpdaterConfiguration AutoUpdater { get; set; } = new();
}

/// <summary>
///     Data type for display configuration.
/// </summary>
public class DisplayConfiguration
{
    /// <summary>
    ///     Game window width in pixels.
    /// </summary>
    public int ResolutionWidth { get; set; }

    /// <summary>
    ///     Game window height in pixels.
    /// </summary>
    public int ResolutionHeight { get; set; }

    /// <summary>
    ///     Fullscreen flag.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    ///     Maximum framerate.
    /// </summary>
    public int MaxFramerate { get; set; }

    /// <summary>
    ///     Font.
    /// </summary>
    public string Font { get; set; } = "";

    /// <summary>
    ///     Base font size.
    /// </summary>
    public float BaseFontSize { get; set; }

    /// <summary>
    ///     Base height for determining the UI scaling factor.
    /// </summary>
    public int BaseScalingHeight { get; set; }
}

/// <summary>
///     Configuration settings for the autoupdater.
/// </summary>
public class AutoUpdaterConfiguration
{
    /// <summary>
    ///     URL of the update server to use.
    /// </summary>
    public string UpdateServerUrl { get; set; } = "";

    /// <summary>
    ///     Whether to run the auto-updater when the client is started.
    /// </summary>
    public bool UpdateOnStartup { get; set; }

    /// <summary>
    ///     If true, prompt the user before beginning the update process.
    /// </summary>
    public bool PromptForUpdate { get; set; }
}