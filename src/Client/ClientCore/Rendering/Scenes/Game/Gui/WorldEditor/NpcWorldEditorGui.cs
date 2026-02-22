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

using Hexa.NET.ImGui;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

public class NpcWorldEditorGui(
    NameComponentCollection names,
    ClientWorldEditServices worldEditServices,
    AnimatedSpriteComponentCollection animatedSprites,
    GuiExtensions guiExtensions,
    ClientWorldEditController worldEditController,
    IEventSender eventSender,
    DrawableComponentCollection drawables,
    IOptions<RendererOptions> rendererOptions)
{
    private const string NoNameLabel = "[no name]";

    private bool snapToGridBuffer;
    private bool snapToGridChangeInProgress;

    /// <summary>
    ///     Renders the NPC world editor controls.
    /// </summary>
    public void Render()
    {
        RenderNpcTemplateControl();
        RenderNpcDrawControls();
    }

    /// <summary>
    ///     Render the control for selecting and displaying the NPC template.
    /// </summary>
    private void RenderNpcTemplateControl()
    {
        if (!ImGui.BeginTable("WorldEditNpcTemplate", 2, ImGuiTableFlags.SizingStretchProp)) return;
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

        var templateName = names.HasComponentForEntity(worldEditServices.NpcTemplateId)
            ? names[worldEditServices.NpcTemplateId]
            : NoNameLabel;

        ImGui.TableNextColumn();
        RenderSpritePreview();

        ImGui.TableNextColumn();
        var relId = worldEditServices.NpcTemplateId - EntityConstants.FirstTemplateEntityId;
        ImGui.Text(templateName);
        ImGui.Text($"NPC Template {relId}");

        ImGui.EndTable();
    }

    /// <summary>
    ///     Renders the sprite preview for the currently selected NPC template.
    /// </summary>
    private void RenderSpritePreview()
    {
        if (!drawables.HasComponentForEntity(worldEditServices.NpcTemplateId) &&
            !animatedSprites.HasComponentForEntity(worldEditServices.NpcTemplateId))
        {
            guiExtensions.Sprite(rendererOptions.Value.DefaultHiddenPlaceholderSprite);
            return;
        }

        var spriteId = animatedSprites[worldEditServices.NpcTemplateId];
        guiExtensions.AnimatedSprite(spriteId, Orientation.South, AnimationPhase.Default);
    }

    /// <summary>
    ///     Renders the controls for drawing NPCs.
    /// </summary>
    private void RenderNpcDrawControls()
    {
        // Update snap-to-grid state if changed.
        if (snapToGridBuffer == worldEditServices.SnapToGrid) snapToGridChangeInProgress = false;
        else if (!snapToGridChangeInProgress) snapToGridBuffer = worldEditServices.SnapToGrid;

        ImGui.Checkbox("Snap to Grid", ref snapToGridBuffer);
        if (snapToGridBuffer != worldEditServices.SnapToGrid)
        {
            worldEditController.SetSnapToGrid(eventSender, snapToGridBuffer);
            snapToGridChangeInProgress = true;
        }
    }

    /// <summary>
    ///     Renders the help text for the NPC tool.
    /// </summary>
    private void RenderNpcToolHelp()
    {
        ImGui.Separator();
        ImGui.Text("Left click to place NPC.");
        ImGui.Text("Right click to remove NPC.");
    }
}