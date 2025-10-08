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

using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Configuration;

/// <summary>
///     Renderer-specific settings.
/// </summary>
public sealed class RendererOptions
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
    ///     Default sprite ID to use as a placeholder for hidden entities when they are shown.
    /// </summary>
    public int DefaultHiddenPlaceholderSprite { get; set; } = 1;

    /// <summary>
    ///     Default opacity for placeholder sprites used when drawing hidden entities.
    /// </summary>
    public float DefaultHiddenPlaceholderOpacity { get; set; } = 0.8f;
}

/// <summary>
///     User-configurable display settings.
/// </summary>
public class DisplayOptions
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
    ///     Whether to sync to vertical blank.
    /// </summary>
    public bool UseVSync { get; set; }

    /// <summary>
    ///     Whether to use the SRGB color profile.
    /// </summary>
    public bool UseSrgb { get; set; }

    /// <summary>
    ///     Whether to enable debug options for the graphics API.
    /// </summary>
    public bool UseGraphicsDebug { get; set; }

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
    ///     Bold font.
    /// </summary>
    public string BoldFont { get; set; } = "";

    /// <summary>
    ///     Icon font.
    /// </summary>
    public string IconFont { get; set; } = "";

    /// <summary>
    ///     Emoji font.
    /// </summary>
    public string EmojiFont { get; set; } = "";

    /// <summary>
    ///     Base font size.
    /// </summary>
    public float BaseFontSize { get; set; }

    /// <summary>
    ///     Base dialogue font size.
    /// </summary>
    public float DialogueFontSize { get; set; }

    /// <summary>
    ///     Base item label font size.
    /// </summary>
    public float ItemLabelFontSize { get; set; }

    /// <summary>
    ///     Base height for determining the UI scaling factor.
    /// </summary>
    public int BaseScalingHeight { get; set; }

    /// <summary>
    ///     Dialogue text speed in chars per second.
    /// </summary>
    public ulong TextSpeedCharsPerSecond { get; set; }
}

/// <summary>
///     Configuration settings for the autoupdater.
/// </summary>
public class AutoUpdaterOptions
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

/// <summary>
///     Configuration options for connection details.
/// </summary>
public sealed class ConnectionOptions
{
    public const string DefaultHost = "127.0.0.1";

    public const ushort DefaultPort = 12820;

    public const string DefaultRestHost = "127.0.0.1";

    public const ushort DefaultRestPort = 8080;

    public const bool DefaultRestTls = false;

    /// <summary>
    ///     Server hostname.
    /// </summary>
    public string Host { get; set; } = DefaultHost;

    /// <summary>
    ///     Server port.
    /// </summary>
    public ushort Port { get; set; } = DefaultPort;

    /// <summary>
    ///     REST server hostname. Typically the same as Host.
    /// </summary>
    public string RestHost { get; set; } = DefaultRestHost;

    /// <summary>
    ///     REST server port.
    /// </summary>
    public ushort RestPort { get; set; } = DefaultRestPort;

    /// <summary>
    ///     Whether the REST server is using a TLS-encrypted connection.
    /// </summary>
    public bool RestTls { get; set; } = DefaultRestTls;
}

/// <summary>
///     Describes a step of the day/night global light progression.
/// </summary>
public sealed class GlobalLightStep
{
    /// <summary>
    ///     Second of the day at which this step begins.
    /// </summary>
    public uint SecondOfDay { get; set; }

    /// <summary>
    ///     Red component of the global light.
    /// </summary>
    public float Red { get; set; }

    /// <summary>
    ///     Green component of the global light.
    /// </summary>
    public float Green { get; set; }

    /// <summary>
    ///     Blue component of the global light.
    /// </summary>
    public float Blue { get; set; }

    /// <summary>
    ///     Convenience property to get the global light color as a Vector3.
    /// </summary>
    public Vector3 Color => new(Red, Green, Blue);
}

/// <summary>
///     Options for the day/night cycle.
/// </summary>
public sealed class DayNightOptions
{
    /// <summary>
    ///     Global light steps in the day/night cycle. Must be monotonically
    ///     increasing in SecondOfDay.
    /// </summary>
    public List<GlobalLightStep> GlobalLightSteps { get; set; } = new()
    {
        new GlobalLightStep
        {
            SecondOfDay = 0,
            Red = 1.0f,
            Green = 1.0f,
            Blue = 1.0f
        }
    };
}

/// <summary>
///     User-configurable options for game editor functions.
/// </summary>
public sealed class EditorOptions
{
    /// <summary>
    ///     Default definition for newly added block template entities.
    /// </summary>
    public EntityDefinition DefaultNewBlockTemplate { get; set; } = new()
    {
        Material = new MaterialPair(1, 0),
        CastBlockShadows = true,
        Name = "New Block"
    };

    /// <summary>
    ///     Default definition for newly added NPC template entities.
    /// </summary>
    public EntityDefinition DefaultNewNpcTemplate { get; set; } = new()
    {
        AnimatedSpriteId = 0,
        Drawable = Vector2.Zero,
        CastShadows = new Shadow
        {
            Radius = 0.1f
        },
        Name = "New NPC"
    };
}