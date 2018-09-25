/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

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
