using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    public sealed class TileSprite
    {

        /// <summary>
        /// Tile sprite ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tile contexts.
        /// </summary>
        public IList<TileContext> TileContexts { get; set; }

    }

}
