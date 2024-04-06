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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Resource editor tab for Animated Sprites.
/// </summary>
public class AnimatedSpriteEditorTab
{
    /// <summary>
    ///     Color for selected rwo in the browser.
    /// </summary>
    private const uint SelectionColor = 0xFF773333;

    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly GuiExtensions guiExtensions;

    /// <summary>
    ///     All orientations in display order.
    /// </summary>
    private readonly List<Orientation> orderedOrientations = new()
    {
        Orientation.South,
        Orientation.Southwest,
        Orientation.West,
        Orientation.Northwest,
        Orientation.North,
        Orientation.Northeast,
        Orientation.East,
        Orientation.Southeast
    };

    private readonly SpriteManager spriteManager;

    private readonly SpriteSelectorPopup spriteSelectorPopup;
    private readonly TileSpriteManager tileSpriteManager;

    /// <summary>
    ///     Frame being edited when the sprite selector is open.
    /// </summary>
    private int editingFrame;

    /// <summary>
    ///     Orientation being edited when the sprite selector is open.
    /// </summary>
    private Orientation editingOrientation;

    private AnimatedSprite? editingSprite;

    /// <summary>
    ///     Initialization flag.
    /// </summary>
    private bool initialized;

    /// <summary>
    ///     Input value for animation timestep in milliseconds.
    /// </summary>
    private float inputTimestepMs;

    /// <summary>
    ///     Currently selected animated sprite ID.
    /// </summary>
    private int selectedId;

    /// <summary>
    ///     Tile sprites that depend on the currently selected animated sprite.
    /// </summary>
    private List<int> tileSpriteDependencies = new();

    public AnimatedSpriteEditorTab(AnimatedSpriteManager animatedSpriteManager, GuiExtensions guiExtensions,
        SpriteSelectorPopup spriteSelectorPopup, SpriteManager spriteManager, TileSpriteManager tileSpriteManager)
    {
        this.animatedSpriteManager = animatedSpriteManager;
        this.guiExtensions = guiExtensions;
        this.spriteSelectorPopup = spriteSelectorPopup;
        this.spriteManager = spriteManager;
        this.tileSpriteManager = tileSpriteManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Renders the Animated Sprites editor tab.
    /// </summary>
    public void Render()
    {
        if (!initialized)
        {
            Select(0);
            initialized = true;
        }

        if (ImGui.BeginTabItem("Animated Sprites"))
        {
            if (ImGui.BeginTable("animSprOuter", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("#browser", ImGuiTableColumnFlags.WidthFixed, 220.0f);
                ImGui.TableSetupColumn("#editor", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                RenderBrowser();

                ImGui.TableNextColumn();
                RenderEditor();

                ImGui.EndTable();
            }

            ImGui.EndTabItem();
        }
    }

    /// <summary>
    ///     Renders the animated sprite browser that allows the user to select an animated sprite for editing.
    /// </summary>
    private void RenderBrowser()
    {
        // Browser selector.
        var maxSize = ImGui.GetWindowSize();
        if (ImGui.BeginTable("animSprBrowser", 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.RowBg, new Vector2 { X = 222.0f, Y = maxSize.Y - 90 }))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Animated Sprite");
            ImGui.TableHeadersRow();

            for (var i = 0; i < animatedSpriteManager.AnimatedSprites.Count; ++i)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                if (guiExtensions.AnimatedSpriteButton($"##spriteButton{i}", i)) Select(i);

                if (i == selectedId)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, SelectionColor);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, SelectionColor);
                }
            }

            ImGui.EndTable();

            // Bottom control row.
            if (ImGui.BeginTable("browserControls", 3, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##Span", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();

                // Insert new sprite button.
                ImGui.TableNextColumn();
                if (ImGui.Button("+")) InsertNewAnimatedSprite();
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text("Insert New");
                        ImGui.EndTooltip();
                    }

                // Remove selected sprite button.
                ImGui.TableNextColumn();
                var canRemove = CanRemoveSprite(out var reason);
                if (!canRemove) ImGui.BeginDisabled();
                if (ImGui.Button("-")) RemoveSelectedAnimatedSprite();
                if (!canRemove) ImGui.EndDisabled();
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text(canRemove ? "Remove Selected" : reason);
                        ImGui.EndTooltip();
                    }


                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    ///     Renders the animated sprite editor for the currently selected animated sprite.
    /// </summary>
    private void RenderEditor()
    {
        if (editingSprite == null)
        {
            Logger.Error("RenderEditor(): editingSprite is null.");
            return;
        }

        ImGui.Text($"Animated Sprite {selectedId}");

        ImGui.Separator();

        ImGui.Text("Animation Timestep:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120.0f);
        ImGui.InputFloat("ms##timestep", ref inputTimestepMs);

        var maxSize = ImGui.GetWindowSize();
        var maxFrames = editingSprite.Faces.Select(p => p.Value.Count).Max();
        if (ImGui.BeginTable("frameEdtior", maxFrames + 2,
                ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg,
                new Vector2 { X = maxSize.X - 276, Y = maxSize.Y - 134 }))
        {
            ImGui.TableSetupColumn("Orientation");
            ImGui.TableSetupColumn("+/-");
            for (var i = 0; i < maxFrames; ++i) ImGui.TableSetupColumn($"Frame {i + 1}");
            ImGui.TableHeadersRow();

            foreach (var orientation in orderedOrientations)
            {
                ImGui.TableNextColumn();
                ImGui.Text(orientation.ToString());

                ImGui.TableNextColumn();
                if (ImGui.Button($"+##{orientation}")) AppendFrame(orientation);
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Add Frame to End ({orientation})");
                        ImGui.EndTooltip();
                    }

                var hasFrames = editingSprite.Faces.ContainsKey(orientation);
                if (!hasFrames) ImGui.BeginDisabled();
                if (ImGui.Button($"-##{orientation}")) RemoveFrame(orientation);
                if (!hasFrames) ImGui.EndDisabled();
                if (ImGui.IsItemHovered())
                    if (ImGui.BeginTooltip())
                    {
                        ImGui.Text($"Remove Frame from End ({orientation})");
                        ImGui.EndTooltip();
                    }

                for (var i = 0; i < maxFrames; ++i)
                {
                    ImGui.TableNextColumn();
                    if (editingSprite.Faces.TryGetValue(orientation, out var frames) && i < frames.Count)
                        if (guiExtensions.SpriteButton($"##{orientation}{i}", frames[i].Id))
                        {
                            editingOrientation = orientation;
                            editingFrame = i;
                            spriteSelectorPopup.Open();
                        }
                }
            }

            HandleSpriteSelector();
            ImGui.EndTable();
        }

        if (ImGui.Button("Save")) SaveState();
        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ResetState();
    }

    /// <summary>
    ///     Selects an animated sprite and sets up all input fields.
    /// </summary>
    /// <param name="id"></param>
    private void Select(int id)
    {
        selectedId = id;
        ResetState();

        tileSpriteDependencies = tileSpriteManager.TileSprites
            .SelectMany(ts => ts.TileContexts
                .SelectMany(ctx => ctx.AnimatedSpriteIds)
                .Select(animatedSpriteId => Tuple.Create(ts.Id, animatedSpriteId)))
            .Where(ids => ids.Item2 == selectedId)
            .Select(ids => ids.Item1)
            .ToList();
    }

    /// <summary>
    ///     Saves the currently edited sprite into the active animated sprite table.
    /// </summary>
    private void SaveState()
    {
        if (editingSprite == null)
        {
            Logger.Error("SaveState(): editingSprite is null.");
            return;
        }

        animatedSpriteManager.Update(selectedId, editingSprite);
    }

    /// <summary>
    ///     Resets state to match the currently applied definition of the selected animated sprite.
    /// </summary>
    private void ResetState()
    {
        inputTimestepMs = animatedSpriteManager.AnimatedSprites[selectedId].FrameTime * UnitConversions.UsToMs;
        editingSprite = new AnimatedSprite(animatedSpriteManager.AnimatedSprites[selectedId]);
    }

    /// <summary>
    ///     Renders the sprite selector if open, and processes results when sprite selected.
    /// </summary>
    private void HandleSpriteSelector()
    {
        if (editingSprite == null)
        {
            Logger.Error("HandleSpriteSelector(): editingSprite is null");
            return;
        }

        spriteSelectorPopup.Render();
        if (spriteSelectorPopup.TryGetSelection(out var newSpriteId))
            // Selection made, update the working record.
            editingSprite.Faces[editingOrientation][editingFrame] = spriteManager.Sprites[newSpriteId];
    }

    /// <summary>
    ///     Appends a new frame to the given orientation of an animated sprite.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    private void AppendFrame(Orientation orientation)
    {
        if (editingSprite == null)
        {
            Logger.Error("AppendFrame(): editingSprite is null");
            return;
        }

        if (!editingSprite.Faces.TryGetValue(orientation, out var frames))
        {
            frames = new List<Sprite>();
            editingSprite.Faces[orientation] = frames;
        }

        frames.Add(spriteManager.Sprites[0]);
    }

    /// <summary>
    ///     Removes the last frame from the given orientation of an animated sprite.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    private void RemoveFrame(Orientation orientation)
    {
        if (editingSprite == null)
        {
            Logger.Error("RemoveFrame(): editingSprite is null");
            return;
        }

        if (!editingSprite.Faces.TryGetValue(orientation, out var frames)) return;

        frames.RemoveAt(frames.Count - 1);
        if (frames.Count == 0) editingSprite.Faces.Remove(orientation);
    }

    /// <summary>
    ///     Inserts a new animated sprite after the currently selected sprite.
    /// </summary>
    private void InsertNewAnimatedSprite()
    {
        animatedSpriteManager.InsertNew(selectedId + 1);
    }

    /// <summary>
    ///     Removes the currently selected animated sprite.
    /// </summary>
    private void RemoveSelectedAnimatedSprite()
    {
        animatedSpriteManager.Remove(selectedId);
        if (selectedId >= animatedSpriteManager.AnimatedSprites.Count)
            Select(animatedSpriteManager.AnimatedSprites.Count - 1);
    }

    /// <summary>
    ///     Checks whether the selected animated sprite may be removed.
    /// </summary>
    /// <param name="reason">Reason the sprite may not be removed. Only valid when returning false.</param>
    /// <returns></returns>
    private bool CanRemoveSprite([NotNullWhen(false)] out string? reason)
    {
        if (animatedSpriteManager.AnimatedSprites.Count <= 1)
        {
            reason = "Cannot remove last animated sprite.";
            return false;
        }

        if (tileSpriteDependencies.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("Cannot remove with dependencies:");
            foreach (var tileSpriteId in tileSpriteDependencies) sb.Append($"\nTile Sprite {tileSpriteId}");

            reason = sb.ToString();
            return false;
        }

        reason = null;
        return true;
    }
}