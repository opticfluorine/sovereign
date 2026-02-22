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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Components.Indexers;
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
    private readonly List<(ulong, string)> filtered = new();
    private readonly GuiExtensions guiExtensions;
    private readonly NameComponentCollection nameComponentCollection;
    private readonly NpcTemplateEntityIndexer npcTemplateEntityIndexer;
    private readonly Vector2 preferredSize = new(15.0f, 20.0f);
    private Vector2 basePos = Vector2.Zero;
    private string instanceKey = string.Empty;
    private bool isSelected;
    private string newSearch = string.Empty;
    private string search = string.Empty;
    private ulong selection;

    public NpcTemplateSelectorPopup(
        NpcTemplateEntityIndexer npcTemplateEntityIndexer,
        GuiExtensions guiExtensions,
        NameComponentCollection nameComponentCollection,
        AnimatedSpriteComponentCollection animatedSprites)
    {
        this.npcTemplateEntityIndexer = npcTemplateEntityIndexer;
        this.guiExtensions = guiExtensions;
        this.nameComponentCollection = nameComponentCollection;
        this.animatedSprites = animatedSprites;
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
        newSearch = string.Empty;
        UpdateFiltered();
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

        ImGui.InputText("\U0001f50d##Search", ref newSearch, 64);
        ImGui.SetItemDefaultFocus();
        if (newSearch != search)
        {
            search = newSearch;
            UpdateFiltered();
        }

        // Matching template list.
        ImGui.BeginChild("npcList", new Vector2(realSize.X, realSize.Y - 40));
        foreach (var (entityId, name) in filtered)
        {
            ImGui.PushID((int)(entityId & 0xFFFFFFFF));
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            if (!animatedSprites.TryGetValue(entityId, out var animatedSpriteId)) animatedSpriteId = 0;
            guiExtensions.AnimatedSprite(animatedSpriteId, Orientation.South, AnimationPhase.Default);

            var spriteSize =
                guiExtensions.CalcAnimatedSpriteSize(animatedSpriteId, Orientation.South, AnimationPhase.Default);
            var label = $"{name} [{entityId - EntityConstants.FirstTemplateEntityId}]";
            var textSize = ImGui.CalcTextSize(label);

            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 0.5f * (spriteSize.Y - textSize.Y));
            if (ImGui.Selectable(label, false))
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

    /// <summary>
    ///     Updates the filtered list of NPC templates.
    /// </summary>
    private void UpdateFiltered()
    {
        var update = npcTemplateEntityIndexer.NpcTemplateEntities
            .Select(entityId => (EntityId: entityId,
                Name: nameComponentCollection.TryGetValue(entityId, out var name) ? name : "[No Name]"))
            .Where(pair =>
                string.IsNullOrEmpty(search) || pair.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(pair => pair.EntityId);

        filtered.Clear();
        filtered.AddRange(update);
    }
}