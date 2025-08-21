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
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Overlay GUI used for displaying information to the player.
/// </summary>
public class OverlayGui(
    DisplayViewport viewport,
    AtlasMap atlasMap,
    CameraServices cameraServices,
    IPerspectiveServices perspectiveServices,
    ClientStateServices clientStateServices,
    NameComponentCollection names,
    EntityTypeComponentCollection entityTypes,
    ILogger<OverlayGui> logger,
    DrawableComponentCollection drawables)
{
    /// <summary>
    ///     Renders the overlay GUI.
    /// </summary>
    /// <param name="renderPlan">Render plan for the current frame.</param>
    public void Render(RenderPlan renderPlan)
    {
        var io = ImGui.GetIO();
        var screenSize = io.DisplaySize;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        try
        {
            ImGui.SetNextWindowPos(new Vector2(0.0f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(screenSize, ImGuiCond.Always);
            if (!ImGui.Begin("Overlay",
                    ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs)) return;

            // Draw name labels.
            var tileScale = new Vector2(screenSize.X / viewport.WidthInTiles,
                screenSize.Y / viewport.HeightInTiles);
            for (var i = 0; i < renderPlan.NameLabelCount; ++i) DrawNameLabel(renderPlan.NameLabels[i], tileScale);

            DrawHoverTooltip();

            ImGui.End();
        }
        finally
        {
            ImGui.PopStyleVar();
        }
    }

    /// <summary>
    ///     Draws the tooltip for the hovered entity if appropriate.
    /// </summary>
    private void DrawHoverTooltip()
    {
        // Get hovered entity.
        var hoverPos = cameraServices.GetMousePositionWorldCoordinates();
        var zStep = viewport.HeightInTiles * 0.5f;
        if (!perspectiveServices.TryGetHighestCoveringEntity(hoverPos, hoverPos.Z - zStep,
                hoverPos.Z + zStep, out var entityId) || EntityUtil.IsBlockEntity(entityId)) return;
        if (clientStateServices.TryGetSelectedPlayer(out var playerId) && entityId == playerId) return;
        if (!entityTypes.TryGetValue(entityId, out var entityType)) return;
        if (!drawables.HasComponentForEntity(entityId) &&
            !clientStateServices.GetStateFlagValue(ClientStateFlag.ShowHiddenEntities)) return;

        switch (entityType)
        {
            case EntityType.Npc:
                DrawNpcTooltip(entityId);
                break;
        }
    }

    /// <summary>
    ///     Draws the hover tooltip for an NPC.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void DrawNpcTooltip(ulong entityId)
    {
        if (!ImGui.BeginTooltip()) return;
        ImGui.PopStyleVar();

        try
        {
            ImGui.Text(names.TryGetValue(entityId, out var name) ? name : "[No Name]");
        }
        finally
        {
            ImGui.EndTooltip();
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }
    }

    /// <summary>
    ///     Draws a single name label to the overlay.
    /// </summary>
    /// <param name="nameLabel">Name label.</param>
    /// <param name="tileScale">Scaling factor for tiles.</param>
    private void DrawNameLabel(NameLabel nameLabel, Vector2 tileScale)
    {
        if (!names.HasComponentForEntity(nameLabel.EntityId))
        {
            logger.LogWarning("No name for entity ID {EntityId:X}.", nameLabel.EntityId);
            return;
        }

        var name = names[nameLabel.EntityId];
        var screenSize = ImGui.GetIO().DisplaySize;

        // Find center of top edge of sprite in screen space.
        var localWorldPos = nameLabel.InterpolatedPosition - cameraServices.Position;
        var spriteWidth = atlasMap.MapElements[nameLabel.SpriteId].WidthInTiles;
        var topEdgeCenterProjWorldPos = new Vector2(
            localWorldPos.X + 0.5f * spriteWidth,
            -(localWorldPos.Y + localWorldPos.Z)
        );
        var topEdgeCenterScreenPos = topEdgeCenterProjWorldPos * tileScale + 0.5f * screenSize;

        // Adjust text positioning to center horizontally, offset vertically.
        var textSize = ImGui.CalcTextSize(name);
        var fontSize = ImGui.GetFontSize();
        var labelPos = topEdgeCenterScreenPos - new Vector2(
            0.5f * textSize.X,
            textSize.Y - 0.0f * fontSize
        );

        // Render label.
        ImGui.SetCursorScreenPos(labelPos);
        ImGui.Text(name);
    }
}