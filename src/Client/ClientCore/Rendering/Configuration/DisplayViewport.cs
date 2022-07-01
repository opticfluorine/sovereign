using Sovereign.ClientCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Configures the display viewport used for rendering.
    /// </summary>
    public sealed class DisplayViewport
    {
        private readonly IClientConfiguration clientConfiguration;

        public DisplayViewport(IClientConfiguration clientConfiguration)
        {
            this.clientConfiguration = clientConfiguration;

            WidthInTiles = (float)Width / clientConfiguration.TileWidth;
            HeightInTiles = (float)Height / clientConfiguration.TileWidth;
        }

        /// <summary>
        /// Width of the display viewport in pixels.
        /// </summary>
        public int Width => 800;

        /// <summary>
        /// Height of the display viewport in pixels.
        /// </summary>
        public int Height => 450;

        /// <summary>
        /// Width of the display viewport as a multiple of the tile width.
        /// </summary>
        public float WidthInTiles { get; private set; }

        /// <summary>
        /// Height of the display viewport as a multiple of the tile height.
        /// </summary>
        public float HeightInTiles { get; private set; }

    }

}
