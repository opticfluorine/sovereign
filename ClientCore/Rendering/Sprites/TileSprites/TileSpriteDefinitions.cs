using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Top-level record of tile sprite definitions.
    /// </summary>
    public sealed class TileSpriteDefinitions
    {

        /// <summary>
        /// Tile sprites.
        /// </summary>
        public IList<TileSprite> TileSprites { get; set; }

    }

}
