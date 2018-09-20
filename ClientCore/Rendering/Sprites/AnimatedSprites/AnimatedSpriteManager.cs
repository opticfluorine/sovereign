using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// Manages the animated sprites.
    /// </summary>
    public sealed class AnimatedSpriteManager
    {

        /// <summary>
        /// Animated sprite definitions loader.
        /// </summary>
        private readonly AnimatedSpriteDefinitionsLoader loader;

        public AnimatedSpriteManager(AnimatedSpriteDefinitionsLoader loader)
        {
            this.loader = loader;
        }

        /// <summary>
        /// Initializes the animated sprites.
        /// </summary>
        public void InitializeAnimatedSprites()
        {

        }

    }

}
