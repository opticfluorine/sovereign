// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Provides utility methods for working with animated sprites.
/// </summary>
public sealed class AnimatedSpriteUtil(
    AnimatedSpriteComponentCollection animatedSprites,
    AnimatedSpriteManager animatedSpriteManager,
    AtlasMap atlasMap,
    ILogger<AnimatedSpriteUtil> logger)
{
    /// <summary>
    ///     Makes an unpositioned BoundingBox for the given entity based on its AnimatedSprite component.
    ///     The resulting bounding box lies in the XY plane and has zero depth.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Bounding box, or BoundingBox.Zero if the animated sprite data is missing or invalid.</returns>
    public BoundingBox MakeXyBoundingBoxForEntity(ulong entityId)
    {
        if (!animatedSprites.TryGetValue(entityId, out var animatedSpriteId))
        {
            logger.LogWarning("No sprite for entity {EntityId:X}.", entityId);
            return BoundingBox.Zero;
        }

        return MakeXyBoundingBoxForSprite(animatedSpriteId);
    }

    /// <summary>
    ///     Makes an unpositioned BoundingBox for the given animated sprite. The resulting box lies in
    ///     the XY plane and has zero depth.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <returns>Bounding box, or BoundingBox.Zero if the animated sprite data is invalid.</returns>
    public BoundingBox MakeXyBoundingBoxForSprite(int animatedSpriteId)
    {
        if (animatedSpriteId < 0 || animatedSpriteId >= animatedSpriteManager.AnimatedSprites.Count)
            return BoundingBox.Zero;

        var sprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].GetDefaultSprite();
        if (sprite.Id < 0 || sprite.Id >= atlasMap.MapElements.Count)
        {
            logger.LogWarning("Animated sprite ID {Id} not found in AtlasMap.", animatedSpriteId);
            return BoundingBox.Zero;
        }

        var spriteInfo = atlasMap.MapElements[sprite.Id];
        return new BoundingBox
        {
            Position = Vector3.Zero,
            Size = new Vector3(spriteInfo.WidthInTiles, spriteInfo.HeightInTiles, 0.0f)
        };
    }
}