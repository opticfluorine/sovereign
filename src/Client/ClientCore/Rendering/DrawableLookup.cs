// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Provides convenience methods for looking up rendering information about drawable entities.
/// </summary>
public class DrawableLookup
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly ClientConfigurationManager configManager;
    private readonly ILogger<DrawableLookup> logger;
    private readonly SpriteSheetManager spriteSheetManager;

    public DrawableLookup(BlockPositionComponentCollection blockPositions,
        AnimatedSpriteComponentCollection animatedSprites,
        AnimatedSpriteManager animatedSpriteManager, SpriteSheetManager spriteSheetManager,
        ClientConfigurationManager configManager, ILogger<DrawableLookup> logger)
    {
        this.blockPositions = blockPositions;
        this.animatedSprites = animatedSprites;
        this.animatedSpriteManager = animatedSpriteManager;
        this.spriteSheetManager = spriteSheetManager;
        this.configManager = configManager;
        this.logger = logger;
    }

    /// <summary>
    ///     Gets the drawable size of the given entity in world units.
    ///     For block entities, this gives the size of a single face.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Drawable size.</returns>
    /// <remarks>
    ///     For animated sprite renderables, this method assumes that all frames of the animation across
    ///     all phases have the same dimensions.
    /// </remarks>
    public Vector2 GetEntityDrawableSizeWorld(ulong entityId)
    {
        if (blockPositions.HasComponentForEntity(entityId))
            // Entity is a block and has a constant size per face.
            return Vector2.One;

        if (!animatedSprites.HasComponentForEntity(entityId))
        {
            logger.LogWarning("Non-block entity {Id:X} has no AnimatedSprite component.", entityId);
            return Vector2.Zero;
        }

        // Determine which sprite will be rendered.
        var animatedSpriteId = animatedSprites[entityId];
        var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
        var sprite = animatedSprite.GetPhaseData(AnimationPhase.Default).GetSpriteForTime(0, Orientation.South);

        // Now look up its size.
        var definition = spriteSheetManager.SpriteSheets[sprite.SpritesheetName].Definition;
        var tileWidth = configManager.ClientConfiguration.TileWidth;
        return new Vector2((float)definition.SpriteWidth / tileWidth, (float)definition.SpriteHeight / tileWidth);
    }
}