using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Configures the display viewport used for rendering.
/// </summary>
public sealed class DisplayViewport
{
    private readonly RendererOptions rendererOptions;

    public DisplayViewport(IOptions<RendererOptions> rendererOptions)
    {
        this.rendererOptions = rendererOptions.Value;
    }

    /// <summary>
    ///     Width of the display viewport in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    ///     Height of the display viewport in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    ///     Width of the display viewport as a multiple of the tile width.
    /// </summary>
    public float WidthInTiles { get; private set; }

    /// <summary>
    ///     Height of the display viewport as a multiple of the tile height.
    /// </summary>
    public float HeightInTiles { get; private set; }

    /// <summary>
    ///     Sets the scale of the display viewport.
    /// </summary>
    /// <param name="screenWidth">Screen width in pixels.</param>
    /// <param name="screenHeight">Screen height in pixels.</param>
    public void UpdateScale(int screenWidth, int screenHeight)
    {
        // Balance between supporting high DPI displays and minimizing texture sampling artifacts.
        var scaleFactor = 1 + screenWidth / 1280;

        Width = screenWidth / scaleFactor;
        Height = screenHeight / scaleFactor;
        WidthInTiles = (float)Width / rendererOptions.TileWidth;
        HeightInTiles = (float)Height / rendererOptions.TileWidth;
    }
}