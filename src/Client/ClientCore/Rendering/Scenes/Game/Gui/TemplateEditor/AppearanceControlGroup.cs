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
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editor control group for appearance-related components.
/// </summary>
public class AppearanceControlGroup(GuiExtensions guiExtensions, AnimatedSpriteSelectorPopup animatedSpriteSelector)
{
    private const uint DefaultPointLightColor = 0xFFFFFFFF;
    private const float DefaultPointLightIntensity = 1.0f;
    private const float DefaultPointLightRadius = 1.0f;
    private const float DefaultShadowRadius = 0.1f;

    private bool inputCastShadows;
    private bool inputPointLightSource;

    /// <summary>
    ///     Renders the appearance control group and updates the given entity definition.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    public void Render(EntityDefinition entityDefinition)
    {
        if (ImGui.CollapsingHeader("Appearance", ImGuiTreeNodeFlags.DefaultOpen))
            if (ImGui.BeginTable("Appearance", 2, ImGuiTableFlags.SizingFixedFit))
            {
                AnimatedSpriteControls(entityDefinition);
                DrawableControls(entityDefinition);
                CastShadowsControls(entityDefinition);
                PointLightSourceControls(entityDefinition);

                ImGui.EndTable();
            }
    }

    /// <summary>
    ///     Renders the AnimatedSprite controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private void AnimatedSpriteControls(EntityDefinition entityDefinition)
    {
        ImGui.TableNextColumn();
        ImGui.Text("Animated Sprite:");
        ImGui.TableNextColumn();
        if (guiExtensions.AnimatedSpriteButton("##animSprEditBtn", entityDefinition.AnimatedSpriteId ?? 0,
                Orientation.South,
                AnimationPhase.Default))
            animatedSpriteSelector.Open();
        ImGui.SameLine();
        ImGui.Text($"Animated Sprite {entityDefinition.AnimatedSpriteId}");

        HandleAnimatedSpriteSelector(entityDefinition);
    }

    /// <summary>
    ///     Renders the Drawable controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private static void DrawableControls(EntityDefinition entityDefinition)
    {
        ImGui.TableNextColumn();
        ImGui.Text("Drawable:");
        ImGui.TableNextColumn();
        var drawable = entityDefinition.Drawable;
        ImGui.Checkbox("##drawable", ref drawable);
        entityDefinition.Drawable = drawable;
    }

    /// <summary>
    ///     Renders the CastShadows controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private void CastShadowsControls(EntityDefinition entityDefinition)
    {
        ImGui.TableNextColumn();
        ImGui.Text("Cast Shadows:");
        ImGui.TableNextColumn();
        ImGui.Checkbox("##castShadows", ref inputCastShadows);

        ImGui.TableNextColumn();
        ImGui.Text("Shadow Radius:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputCastShadows);
        var shadowRadius = entityDefinition.CastShadows?.Radius ?? DefaultShadowRadius;
        ImGui.InputFloat("##shadowRadius", ref shadowRadius, 0.1f, 1.0f, "%.2f");
        ImGui.EndDisabled();

        if (inputCastShadows)
            entityDefinition.CastShadows = new Shadow
            {
                Radius = shadowRadius
            };
        else
            entityDefinition.CastShadows = null;
    }

    /// <summary>
    ///     Renders the PointLightSource controls.
    /// </summary>
    /// <param name="entityDefinition">Entity definition.</param>
    private void PointLightSourceControls(EntityDefinition entityDefinition)
    {
        // Point Light Source
        ImGui.TableNextColumn();
        ImGui.Text("Point Light Source:");
        ImGui.TableNextColumn();
        ImGui.Checkbox("##pointLightSource", ref inputPointLightSource);

        ImGui.TableNextColumn();
        ImGui.Text("Radius:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPointLightSource);
        var pointLightRadius = entityDefinition.PointLightSource?.Radius ?? DefaultPointLightRadius;
        ImGui.InputFloat("##pointLightRadius", ref pointLightRadius, 0.1f, 10.0f, "%.2f");
        ImGui.EndDisabled();

        ImGui.TableNextColumn();
        ImGui.Text("Intensity:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPointLightSource);
        var pointLightIntensity = entityDefinition.PointLightSource?.Intensity ?? DefaultPointLightIntensity;
        ImGui.InputFloat("##pointLightIntensity", ref pointLightIntensity, 0.1f, 10.0f, "%.2f");
        ImGui.EndDisabled();

        ImGui.TableNextColumn();
        ImGui.Text("Color:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPointLightSource);
        var pointLightColor =
            ColorUtil.UnpackColorRgb(entityDefinition.PointLightSource?.Color ?? DefaultPointLightColor);
        ImGui.ColorEdit3("##pointLightColor", ref pointLightColor);
        ImGui.EndDisabled();

        ImGui.TableNextColumn();
        ImGui.Text("Position Offset:");
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(!inputPointLightSource);
        var pointLightPosOffset = entityDefinition.PointLightSource?.PositionOffset ?? Vector3.Zero;
        ImGui.InputFloat3("##pointLightPositionOffset", ref pointLightPosOffset);
        ImGui.EndDisabled();

        if (inputPointLightSource)
            entityDefinition.PointLightSource = new PointLight
            {
                Radius = pointLightRadius,
                Intensity = pointLightIntensity,
                Color = ColorUtil.PackColorRgb(pointLightColor),
                PositionOffset = pointLightPosOffset
            };
        else
            entityDefinition.PointLightSource = null;
    }

    /// <summary>
    ///     Handles the animated sprite selector popup if it is open.
    /// </summary>
    private void HandleAnimatedSpriteSelector(EntityDefinition entityDefinition)
    {
        animatedSpriteSelector.Render();
        if (animatedSpriteSelector.TryGetSelection(out var selectedId))
            entityDefinition.AnimatedSpriteId = selectedId;
    }
}