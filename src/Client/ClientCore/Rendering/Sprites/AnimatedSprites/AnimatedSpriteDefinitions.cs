/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Describes the animated sprite definitions file.
/// </summary>
public sealed class AnimatedSpriteDefinitions
{
    /// <summary>
    ///     List of animated sprite definitions.
    /// </summary>
    public List<AnimatedSpriteDefinition> AnimatedSprites { get; set; } = new();

    /// <summary>
    ///     Single definition of an animated sprite.
    /// </summary>
    public sealed class AnimatedSpriteDefinition
    {
        /// <summary>
        ///     Animated sprite ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Length of time to display each frame, in microseconds.
        /// </summary>
        public ulong AnimationTimestep { get; set; }

        /// <summary>
        ///     Sprite IDs for each frame.
        /// </summary>
        public List<int> SpriteIds { get; set; } = new();
    }
}