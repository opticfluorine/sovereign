using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Describes a context for converting tile sprites to sprites.
    /// </summary>
    public sealed class TileContext
    {

        /// <summary>
        /// Index of the north tile sprite to match (-1 for wildcard).
        /// </summary>
        public int NorthTileSpriteId { get; set; }

        /// <summary>
        /// Index of the east tile sprite to match (-1 for wildcard).
        /// </summary>
        public int EastTileSpriteId { get; set; }

        /// <summary>
        /// Index of the south tile sprite to match (-1 for wildcard).
        /// </summary>
        public int SouthTileSpriteId { get; set; }


        /// <summary>
        /// Index of the west tile sprite to match (-1 for wildcard).
        /// </summary>
        public int WestTileSpriteId { get; set; }

        /// <summary>
        /// List of sprite IDs to be drawn in order if the context matches.
        /// </summary>
        public List<int> SpriteIds { get; set; }

    }

}
