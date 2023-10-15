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
using System.Linq;
using static Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites.AnimatedSpriteDefinitions;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Describes a single animated sprite.
/// </summary>
public sealed class AnimatedSprite
{
    public AnimatedSprite(AnimatedSpriteDefinition definition, SpriteManager spriteManager)
    {
        FrameTime = definition.AnimationTimestep;
        Sprites = definition.SpriteIds
            .Select(id => spriteManager.Sprites[id])
            .ToList();
    }

    /// <summary>
    ///     Time to display each frame, in microseconds.
    /// </summary>
    public ulong FrameTime { get; }

    /// <summary>
    ///     Sprite IDs for each animation frame.
    /// </summary>
    public IList<Sprite> Sprites { get; }

    /// <summary>
    ///     Gets the sprite for the given system time.
    /// </summary>
    /// <param name="systemTime">System time.</param>
    /// <returns>Sprite for the given system time.</returns>
    public Sprite GetSpriteForTime(ulong systemTime)
    {
        var spriteFrame = (int)(systemTime / FrameTime) % Sprites.Count;
        return Sprites[spriteFrame];
    }
}