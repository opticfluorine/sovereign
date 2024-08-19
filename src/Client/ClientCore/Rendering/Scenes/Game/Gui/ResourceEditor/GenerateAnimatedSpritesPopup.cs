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
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Popup GUI for generating animated sprites from a spritesheet.
/// </summary>
public class GenerateAnimatedSpritesPopup
{
    private const string GenerateFromSheetPopupName = "Generate from Sheet";
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly SpriteManager spriteManager;
    private readonly SpritesheetSelector spritesheetSelector;

    public GenerateAnimatedSpritesPopup(SpritesheetSelector spritesheetSelector, SpriteManager spriteManager,
        AnimatedSpriteManager animatedSpriteManager)
    {
        this.spritesheetSelector = spritesheetSelector;
        this.spriteManager = spriteManager;
        this.animatedSpriteManager = animatedSpriteManager;
    }

    /// <summary>
    ///     Opens the popup.
    /// </summary>
    public void Open()
    {
        ImGui.OpenPopup(GenerateFromSheetPopupName);
    }

    /// <summary>
    ///     Renders the popup if open.
    /// </summary>
    public void Render()
    {
        if (ImGui.BeginPopup(GenerateFromSheetPopupName))
        {
            spritesheetSelector.Render();

            ImGui.Separator();

            if (ImGui.BeginTable("gasButtons", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
                if (ImGui.Button("OK"))
                {
                    DoGenerate();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndTable();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Generates animated sprites for the current spritesheet.
    /// </summary>
    private void DoGenerate()
    {
        // This is O(n) in the sprites and could be improved with an index, but should
        // be good enough for now.
        var sprites = spriteManager.Sprites
            .Where(sprite => sprite.SpritesheetName.Equals(spritesheetSelector.SelectedSpritesheetName))
            .OrderBy(sprite => sprite.Row)
            .ThenBy(sprite => sprite.Column);

        foreach (var sourceSprite in sprites)
        {
            var index = animatedSpriteManager.AnimatedSprites.Count;
            animatedSpriteManager.InsertNew(index);
            var animatedSprite = new AnimatedSprite(animatedSpriteManager.AnimatedSprites[index]);
            animatedSprite.Phases[AnimationPhase.Default].Frames[Orientation.South][0] = sourceSprite;
            animatedSpriteManager.Update(index, animatedSprite);
        }
    }
}