using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Points to a single sprite.
    /// </summary>
    public sealed class Sprite
    {

        /// <summary>
        /// Sprite ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Spritesheet filename.
        /// </summary>
        public string SpritesheetName { get; set; }

        /// <summary>
        /// Row containing the sprite.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column containing the sprite.
        /// </summary>
        public int Column { get; set; }

    }

}
