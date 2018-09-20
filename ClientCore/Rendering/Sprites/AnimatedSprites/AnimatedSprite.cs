using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// Describes a single animated sprite.
    /// </summary>
    public sealed class AnimatedSprite
    {

        /// <summary>
        /// Time to display each frame, in microseconds.
        /// </summary>
        public ulong FrameTime { get; private set; }

        /// <summary>
        /// Sprite IDs for each animation frame.
        /// </summary>
        public IList<Sprite> Sprites { get; private set; }

        public AnimatedSprite(ulong frameTime, IList<Sprite> sprites)
        {
            FrameTime = frameTime;
            Sprites = sprites;
        }

    }

}
