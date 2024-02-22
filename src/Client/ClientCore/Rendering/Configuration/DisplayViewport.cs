using Sovereign.ClientCore.Configuration;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Configures the display viewport used for rendering.
/// </summary>
public sealed class DisplayViewport
{
    public DisplayViewport(ClientConfigurationManager configManager)
    {
        WidthInTiles = (float)Width / configManager.ClientConfiguration.TileWidth;
        HeightInTiles = (float)Height / configManager.ClientConfiguration.TileWidth;
    }

    /// <summary>
    ///     Width of the display viewport in pixels.
    /// </summary>
    public int Width => 800;

    /// <summary>
    ///     Height of the display viewport in pixels.
    /// </summary>
    public int Height => 450;

    /// <summary>
    ///     Width of the display viewport as a multiple of the tile width.
    /// </summary>
    public float WidthInTiles { get; private set; }

    /// <summary>
    ///     Height of the display viewport as a multiple of the tile height.
    /// </summary>
    public float HeightInTiles { get; private set; }
}