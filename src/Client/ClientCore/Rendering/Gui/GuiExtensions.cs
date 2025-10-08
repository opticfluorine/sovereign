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

using System.Linq;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Network.Infrastructure;
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
    /// <summary>
    ///     Scale factor to apply to displayed sprites.
    /// </summary>
    public const float SpriteScaleFactor = 0.0833f;

    private readonly ScriptInfoClient scriptInfoClient;

    private readonly GuiTextureMapper textureMapper;

    public GuiExtensions(GuiTextureMapper textureMapper, ScriptInfoClient scriptInfoClient)
    {
        this.textureMapper = textureMapper;
        this.scriptInfoClient = scriptInfoClient;
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
        ImGui.Image(texId, GetSpriteDimensions(texData));
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
        var result = ImGui.ImageButton(id, texId, GetSpriteDimensions(texData));
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
        ImGui.Image(texId, GetSpriteDimensions(texData));
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI scaled to the given size.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    /// <param name="drawSize">Draw size in pixels.</param>
    public void AnimatedSprite(int animatedSpriteId, Orientation orientation, AnimationPhase phase,
        Vector2 drawSize)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId, orientation, phase);

        // Render GUI component.
        ImGui.Image(texId, drawSize);
    }

    /// <summary>
    ///     Calculates the screen space size of an animated sprite.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Phase.</param>
    /// <returns>Sprite size in GUI units.</returns>
    public Vector2 CalcAnimatedSpriteSize(int animatedSpriteId, Orientation orientation, AnimationPhase phase)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId, orientation, phase);
        var texData = textureMapper.GetTextureDataForTextureId(texId);

        return GetSpriteDimensions(texData);
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

        ImGui.Image(texId, GetSpriteDimensions(texData));
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
        var result = ImGui.ImageButton(id, texId, GetSpriteDimensions(texData));
        ImGui.PopStyleVar();

        return result;
    }

    /// <summary>
    ///     Renders an animated sprite to the GUI's foreground layer in front of all other GUI elements.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    /// <param name="position">Top-left screen space position in pixels.</param>
    public void AnimatedSpriteForeground(int animatedSpriteId, Orientation orientation, AnimationPhase phase,
        Vector2 position)
    {
        var texId = textureMapper.GetTextureIdForAnimatedSprite(animatedSpriteId, orientation, phase);
        var texData = textureMapper.GetTextureDataForTextureId(texId);
        var end = position + GetSpriteDimensions(texData);

        ImGui.GetForegroundDrawList().AddImage(texId, position, end);
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

        ImGui.Image(texId, GetSpriteDimensions(texData));
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

        ImGui.Image(texId, GetSpriteDimensions(texData));
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
        var result = ImGui.ImageButton(id, texId, GetSpriteDimensions(texData));
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
        ImGui.Image(texId, GetSpriteDimensions(texData));
    }

    /// <summary>
    ///     Renders a component for selecting a script callback.
    /// </summary>
    /// <param name="id">Unique ID for this component.</param>
    /// <param name="scriptName">Script name input buffer.</param>
    /// <param name="functionName">Callback function name input buffer.</param>
    public void ScriptFunctionSelector(string id, ref string scriptName, ref string functionName, out bool changed)
    {
        changed = false;

        var scripts = scriptInfoClient.ScriptInfo.Scripts;
        var fontSize = ImGui.GetFontSize();
        ImGui.Text("Script:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fontSize * 16.0f);
        if (ImGui.BeginCombo($"##script_{id}", scriptName == string.Empty ? "[None]" : scriptName))
        {
            if (ImGui.Selectable("[None]", scriptName == string.Empty))
            {
                scriptName = string.Empty;
                functionName = string.Empty;
                changed = true;
            }

            foreach (var script in scripts.Where(s => s.Functions.Count > 0))
            {
                var selected = scriptName == script.Name;
                if (ImGui.Selectable(script.Name, selected))
                {
                    scriptName = script.Name;
                    functionName = script.Functions.First().Name;
                    changed = true;
                }

                if (selected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();

        var shouldDisable = string.IsNullOrEmpty(scriptName);
        if (shouldDisable) functionName = "";
        ImGui.BeginDisabled(shouldDisable);
        ImGui.Text("Function:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fontSize * 16.0f);
        if (ImGui.BeginCombo($"##func_{id}", functionName))
        {
            var selectedScriptName = scriptName;
            var script = scripts.First(s => s.Name == selectedScriptName);

            foreach (var function in script.Functions)
            {
                var selected = functionName == function.Name;
                if (ImGui.Selectable(function.Name, selected))
                {
                    functionName = function.Name;
                    changed = true;
                }

                if (selected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        ImGui.EndDisabled();
    }

    /// <summary>
    ///     Converts world unit dimensions to GUI-friendly pixel dimensions.
    /// </summary>
    /// <param name="worldUnits">Size in world units.</param>
    /// <returns>Size in pixels.</returns>
    public Vector2 WorldUnitsToPixels(Vector2 worldUnits)
    {
        var fontSize = ImGui.GetFontSize();
        return fontSize * SpriteScaleFactor * worldUnits;
    }

    /// <summary>
    ///     Gets scaled sprite dimensions for display.
    /// </summary>
    /// <param name="texData">Texture data.</param>
    /// <returns></returns>
    private Vector2 GetSpriteDimensions(GuiTextureMapper.TextureData texData)
    {
        var fontSize = ImGui.GetFontSize();
        return new Vector2(fontSize * SpriteScaleFactor * texData.Width, fontSize * SpriteScaleFactor * texData.Height);
    }
}