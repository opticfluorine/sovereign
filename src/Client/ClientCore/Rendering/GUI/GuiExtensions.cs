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
using ImGuiNET;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components.Types;
using static Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites.AnimatedSprite;

namespace Sovereign.ClientCore.Rendering.GUI;

/// <summary>
///     Provides specialized extensions to Dear ImGui.
/// </summary>
public class GuiExtensions
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AtlasMap atlasMap;
    private readonly ClientConfiguration clientConfiguration;

    public GuiExtensions(AnimatedSpriteManager animatedSpriteManager, AtlasMap atlasMap,
        ClientConfiguration clientConfiguration)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.clientConfiguration = clientConfiguration;
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    public void AnimatedSprite(int animatedSpriteId)
    {
        var texId = ToGuiTextureHandle(animatedSpriteId);

        // For convenience, select dimensions to match the first south-facing sprite.
        var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
        var firstSprite = animatedSprite.GetSpriteForTime(0, Orientation.South);
        var spriteBounds = atlasMap.MapElements[firstSprite.Id];
        var width = spriteBounds.WidthInTiles * clientConfiguration.TileWidth;
        var height = spriteBounds.HeightInTiles * clientConfiguration.TileWidth;

        // Render GUI component.
        ImGui.Image(texId, new Vector2(width, height));
    }
}