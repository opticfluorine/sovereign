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
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.EngineCore.Events;
using Sovereign.EngineUtil.Constants;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

/// <summary>
///     Manages the GUI for the client-side world editor.
/// </summary>
public class WorldEditorGui
{
    private const string BlockToolLabel = $"{Emoji.Mountain}";
    private const string NpcToolLabel = $"{Emoji.BustInSilhouette}";
    private const string ItemToolLabel = $"{Emoji.RedApple}";
    private const string BlockToolCaption = "Blocks";
    private const string NpcToolCaption = "NPCs";
    private const string ItemToolCaption = "Items";

    private readonly BlockWorldEditorGui blockWorldEditorGui;
    private readonly IEventSender eventSender;
    private readonly ItemWorldEditorGui itemWorldEditorGui;
    private readonly NpcWorldEditorGui npcWorldEditorGui;
    private readonly ClientWorldEditController worldEditController;
    private readonly ClientWorldEditServices worldEditServices;

    public WorldEditorGui(ClientWorldEditServices worldEditServices, IEventSender eventSender,
        ClientWorldEditController worldEditController, BlockWorldEditorGui blockWorldEditorGui,
        NpcWorldEditorGui npcWorldEditorGui, ItemWorldEditorGui itemWorldEditorGui)
    {
        this.worldEditServices = worldEditServices;
        this.eventSender = eventSender;
        this.worldEditController = worldEditController;
        this.blockWorldEditorGui = blockWorldEditorGui;
        this.npcWorldEditorGui = npcWorldEditorGui;
        this.itemWorldEditorGui = itemWorldEditorGui;
    }

    /// <summary>
    ///     Renders the world editor GUI.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(fontSize * new Vector2(15.0f, 15.5f));
        if (!ImGui.Begin("World Editor", ImGuiWindowFlags.NoResize)) return;

        RenderToolSelection();

        switch (worldEditServices.WorldEditTool)
        {
            case WorldEditTool.Block:
                blockWorldEditorGui.Render();
                break;

            case WorldEditTool.Npc:
                npcWorldEditorGui.Render();
                break;

            case WorldEditTool.Item:
                itemWorldEditorGui.Render();
                break;
        }

        ImGui.End();
    }

    /// <summary>
    ///     Renders the tool selection buttons for the world editor.
    /// </summary>
    private void RenderToolSelection()
    {
        PushToolButtonColor(WorldEditTool.Block);
        if (ImGui.Button(BlockToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Block);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text(BlockToolCaption);
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        PushToolButtonColor(WorldEditTool.Npc);
        if (ImGui.Button(NpcToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Npc);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text(NpcToolCaption);
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        PushToolButtonColor(WorldEditTool.Item);
        if (ImGui.Button(ItemToolLabel)) worldEditController.SetTool(eventSender, WorldEditTool.Item);
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
        {
            ImGui.Text(ItemToolCaption);
            ImGui.EndTooltip();
        }

        ImGui.Separator();
    }

    /// <summary>
    ///     Pushes the style color for the tool button based on whether it is selected or not.
    /// </summary>
    /// <param name="worldEditTool">Tool.</param>
    private void PushToolButtonColor(WorldEditTool worldEditTool)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,
            worldEditServices.WorldEditTool == worldEditTool
                ? WorldEditorConstants.SelectedColor
                : WorldEditorConstants.UnselectedColor);
    }
}