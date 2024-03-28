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

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Provides specialized extensions to Dear ImGui.
/// </summary>
public class GuiExtensions
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AtlasMap atlasMap;
    private readonly ClientConfigurationManager clientConfigurationManager;
    private readonly GuiTextureMapper textureMapper;

    public GuiExtensions(AnimatedSpriteManager animatedSpriteManager, AtlasMap atlasMap,
        ClientConfigurationManager clientConfigurationManager, GuiTextureMapper textureMapper)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.clientConfigurationManager = clientConfigurationManager;
        this.textureMapper = textureMapper;
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    public void AnimatedSprite(int animatedSpriteId)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        // Render GUI component.
        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders a full spritesheet to the GUI.
    /// </summary>
    /// <param name="spritesheet">Spritesheet name.</param>
    public void Spritesheet(string spritesheet)
    {
        var texId = textureMapper.GetTextureIdForSpritesheet(spritesheet);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        // Render GUI component.
        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }
}