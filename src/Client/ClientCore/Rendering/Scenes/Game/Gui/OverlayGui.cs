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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineCore.Components;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Overlay GUI used for displaying information to the player.
/// </summary>
public class OverlayGui
{
    private readonly AtlasMap atlasMap;
    private readonly CameraServices cameraServices;
    private readonly ILogger<OverlayGui> logger;
    private readonly NameComponentCollection names;
    private readonly DisplayViewport viewport;

    public OverlayGui(DisplayViewport viewport, AtlasMap atlasMap, CameraServices cameraServices,
        NameComponentCollection names, ILogger<OverlayGui> logger)
    {
        this.viewport = viewport;
        this.atlasMap = atlasMap;
        this.cameraServices = cameraServices;
        this.names = names;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the overlay GUI.
    /// </summary>
    /// <param name="renderPlan">Render plan for the current frame.</param>
    public void Render(RenderPlan renderPlan)
    {
        var io = ImGui.GetIO();
        var screenSize = io.DisplaySize;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f));
        try
        {
            ImGui.SetNextWindowPos(new Vector2(0.0f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(screenSize, ImGuiCond.Always);
            if (!ImGui.Begin("Overlay",
                    ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs)) return;

            var tileScale = new Vector2(screenSize.X / viewport.WidthInTiles,
                screenSize.Y / viewport.HeightInTiles);
            for (var i = 0; i < renderPlan.NameLabelCount; ++i) DrawNameLabel(renderPlan.NameLabels[i], tileScale);

            ImGui.End();
        }
        finally
        {
            ImGui.PopStyleVar();
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
            logger.LogWarning("No name for entity ID {EntityId}.", nameLabel.EntityId);
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