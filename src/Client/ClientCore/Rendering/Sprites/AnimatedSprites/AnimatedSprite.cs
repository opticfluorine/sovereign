﻿/*
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

using System.Collections.Generic;
using System.Linq;
using static Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites.AnimatedSpriteDefinitions;

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

        public AnimatedSprite(AnimatedSpriteDefinition definition, SpriteManager spriteManager)
        {
            FrameTime = definition.AnimationTimestep;
            Sprites = definition.SpriteIds
                .Select(id => spriteManager.Sprites[id])
                .ToList();
        }

        /// <summary>
        /// Gets the sprite for the given system time.
        /// </summary>
        /// <param name="systemTime">System time.</param>
        /// <returns>Sprite for the given system time.</returns>
        public Sprite GetSpriteForTime(ulong systemTime)
        {
            var spriteFrame = (int)(systemTime / FrameTime) % Sprites.Count;
            return Sprites[spriteFrame];
        }

    }

}
