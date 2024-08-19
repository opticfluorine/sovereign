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

using ImGuiNET;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components.Types;
using Vector2 = System.Numerics.Vector2;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Provides specialized extensions to Dear ImGui.
/// </summary>
public class GuiExtensions
{
    private readonly GuiTextureMapper textureMapper;

    public GuiExtensions(GuiTextureMapper textureMapper)
    {
        this.textureMapper = textureMapper;
    }

    /// <summary>
    ///     Renders a sprite to the GUI.
    /// </summary>
    /// <param name="spriteId"></param>
    public void Sprite(int spriteId)
    {
        var texId = textureMapper.GetTextureIdForSprite(spriteId);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        // Render GUI component.
        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders a static sprite to the GUI as a clickable button.
    /// </summary>
    /// <param name="id">Button ID.</param>
    /// <param name="spriteId">Sprite ID.</param>
    /// <returns>true if button clicked, false otherwise.</returns>
    public bool SpriteButton(string id, int spriteId)
    {
        var texId = textureMapper.GetTextureIdForSprite(spriteId);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        var result = ImGui.ImageButton(id, texId, new Vector2(texData.Width, texData.Height));
        ImGui.PopStyleVar();

        return result;
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    public void AnimatedSprite(int animatedSpriteId, Orientation orientation, AnimationPhase phase)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId, orientation, phase);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        // Render GUI component.
        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI.
    /// </summary>
    /// <param name="customId">Unique identifier for custom animated sprite.</param>
    /// <param name="customSprite">Custom sprite.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    public void AnimatedSprite(string customId, AnimatedSprite customSprite, Orientation orientation,
        AnimationPhase phase)
    {
        var texId = textureMapper.GetTextureIdForCustomAnimatedSprite(customId, customSprite, orientation, phase);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI as a clickable button.
    /// </summary>
    /// <param name="id">Button ID.</param>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    /// <returns>true if button clicked, false otherwise.</returns>
    public bool AnimatedSpriteButton(string id, int animatedSpriteId, Orientation orientation, AnimationPhase phase)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId, orientation, phase);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        var result = ImGui.ImageButton(id, texId, new Vector2(texData.Width, texData.Height));
        ImGui.PopStyleVar();

        return result;
    }

    /// <summary>
    ///     Renders a tile sprite in a given context to the GUI.
    /// </summary>
    /// <param name="tileSpriteId">Tile sprite ID.</param>
    /// <param name="contextKey">Tile context key.</param>
    public void TileSprite(int tileSpriteId, TileContextKey contextKey)
    {
        var texId = textureMapper.GetTextureIdForTileSprite(tileSpriteId, contextKey);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders a custom tile sprite in a given context to the GUI.
    /// </summary>
    /// <param name="customId">Unique identifier for the custom sprite.</param>
    /// <param name="customSprite">Custom tile sprite.</param>
    /// <param name="contextKey">Tile context key.</param>
    public void TileSprite(string customId, TileSprite customSprite, TileContextKey contextKey)
    {
        var texId = textureMapper.GetTextureIdForCustomTileSprite(customId, customSprite, contextKey);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.Image(texId, new Vector2(texData.Width, texData.Height));
    }

    /// <summary>
    ///     Renders a tile sprite in a given context to the GUI as a clickable button.
    /// </summary>
    /// <param name="id">Button ID.</param>
    /// <param name="tileSpriteId">Tile sprite ID.</param>
    /// <param name="contextKey">Tile context key.</param>
    /// <returns>true if clicked, false otherwise.</returns>
    public bool TileSpriteButton(string id, int tileSpriteId, TileContextKey contextKey)
    {
        var texId = textureMapper.GetTextureIdForTileSprite(tileSpriteId, contextKey);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        var result = ImGui.ImageButton(id, texId, new Vector2(texData.Width, texData.Height));
        ImGui.PopStyleVar();

        return result;
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