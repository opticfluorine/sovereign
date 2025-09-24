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

using System;
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Controls;

/// <summary>
///     Popup for selecting a NPC template entity.
/// </summary>
public class NpcTemplateSelectorPopup
{
    private const string PopupName = "Select NPC Template Entity";
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly GuiExtensions guiExtensions;
    private readonly NameComponentCollection nameComponentCollection;
    private readonly NpcTemplateEntityIndexer npcTemplateEntityIndexer;
    private readonly Vector2 preferredSize = new(15.0f, 20.0f);
    private readonly RendererOptions rendererOptions;
    private Vector2 basePos = Vector2.Zero;
    private string instanceKey = string.Empty;
    private bool isSelected;
    private string search = string.Empty;
    private ulong selection;

    public NpcTemplateSelectorPopup(
        NpcTemplateEntityIndexer npcTemplateEntityIndexer,
        GuiExtensions guiExtensions,
        NameComponentCollection nameComponentCollection,
        AnimatedSpriteComponentCollection animatedSprites,
        IOptions<RendererOptions> rendererOptions)
    {
        this.npcTemplateEntityIndexer = npcTemplateEntityIndexer;
        this.guiExtensions = guiExtensions;
        this.nameComponentCollection = nameComponentCollection;
        this.animatedSprites = animatedSprites;
        this.rendererOptions = rendererOptions.Value;
    }

    /// <summary>
    ///     Opens the selector popup.
    /// </summary>
    /// <param name="key">Instance key.</param>
    public void Open(string key)
    {
        isSelected = false;
        selection = 0;
        search = string.Empty;
        basePos = ImGui.GetMousePos();
        var scaledSize = ImGui.GetFontSize() * preferredSize;
        if (basePos.Y + scaledSize.Y > 0.95f * ImGui.GetIO().DisplaySize.Y)
            basePos.Y = Math.Max(0.0f, basePos.Y - 1.1f * scaledSize.Y);

        instanceKey = key;
        ImGui.OpenPopup(PopupName);
    }

    /// <summary>
    ///     Renders the selector popup if open, otherwise does nothing.
    /// </summary>
    public void Render()
    {
        var scale = ImGui.GetFontSize();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 0.25f * scale * Vector2.One);
        if (!ImGui.BeginPopup(PopupName))
        {
            ImGui.PopStyleVar();
            return;
        }

        var screenSize = ImGui.GetIO().DisplaySize;
        var maxSize = new Vector2(screenSize.X - basePos.X - 16, screenSize.Y - basePos.Y - 128);
        var realSize = new Vector2(Math.Min(scale * preferredSize.X, maxSize.X),
            Math.Min(scale * preferredSize.Y, maxSize.Y));

        ImGui.SetNextWindowSize(realSize, ImGuiCond.Always);

        // Search box
        ImGui.InputText("\U0001f50d##Search", ref search, 64);

        // Filtered list
        ImGui.BeginChild("npcList", new Vector2(realSize.X, realSize.Y - 40));
        var lowerSearch = search.ToLower();
        var filtered = npcTemplateEntityIndexer.NpcTemplateEntities
            .Select(entityId => (EntityId: entityId,
                Name: nameComponentCollection.TryGetValue(entityId, out var name) ? name : "[No Name]"))
            .Where(pair => string.IsNullOrEmpty(search) || pair.Name.ToLower().Contains(lowerSearch))
            .OrderBy(pair => pair.EntityId)
            .ToList();

        foreach (var (entityId, name) in filtered)
        {
            ImGui.PushID((int)(entityId & 0xFFFFFFFF));
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            guiExtensions.AnimatedSprite(animatedSprites.TryGetValue(entityId, out var animatedSpriteId)
                    ? animatedSpriteId
                    : rendererOptions.DefaultHiddenPlaceholderSprite,
                Orientation.South,
                AnimationPhase.Default);

            ImGui.SameLine();
            if (ImGui.Selectable($"{name} [{entityId - EntityConstants.FirstTemplateEntityId}]", false))
            {
                selection = entityId;
                isSelected = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndGroup();
            ImGui.PopID();
        }

        ImGui.EndChild();
        ImGui.EndPopup();
        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Tries to get the latest selection.
    /// </summary>
    /// <param name="key">Instance key.</param>
    /// <param name="templateEntityId">Set to the selected NPC template entity ID if returns true.</param>
    /// <returns>true if a selection has been made since the last call to Open(); false otherwise.</returns>
    public bool TryGetSelection(string key, out ulong templateEntityId)
    {
        templateEntityId = 0;
        if (key != instanceKey) return false;

        templateEntityId = selection;
        var result = isSelected;
        isSelected = false;
        return result;
    }
}