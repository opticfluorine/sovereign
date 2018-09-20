using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// Describes the animated sprite definitions file.
    /// </summary>
    public sealed class AnimatedSpriteDefinitions
    {

        /// <summary>
        /// List of animated sprite definitions.
        /// </summary>
        public IList<AnimatedSpriteDefinition> AnimatedSprites { get; set; }

        /// <summary>
        /// Single definition of an animated sprite.
        /// </summary>
        public sealed class AnimatedSpriteDefinition
        {

            /// <summary>
            /// Animated sprite ID.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Length of time to display each frame, in microseconds.
            /// </summary>
            public ulong AnimationTimestep { get; set; }

            /// <summary>
            /// Sprite IDs for each frame.
            /// </summary>
            public IList<int> SpriteIds { get; set; }

        }

    }

}
