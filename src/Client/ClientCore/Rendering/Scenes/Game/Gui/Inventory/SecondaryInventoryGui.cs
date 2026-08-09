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
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;

/// <summary>
///     Renders the inventory of a secondary entity (e.g. an NPC) in a separate, closable window.
/// </summary>
public sealed class SecondaryInventoryGui(
    InventoryGridRenderer gridRenderer,
    ClientStateServices stateServices,
    ClientStateController stateController,
    IEventSender eventSender,
    NameComponentCollection names,
    GuiExtensions guiExtensions)
{
    /// <summary>
    ///     Renders the secondary inventory GUI, if one is selected.
    /// </summary>
    public void Render()
    {
        if (!stateServices.TryGetSecondaryInventoryEntity(out var entityId)) return;
        var itemSize = guiExtensions.WorldUnitsToPixels(Vector2.One);
        var name = names.TryGetValue(entityId, out var n) ? n : "[unknown]";
        var open = true;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.025f * itemSize.X));
        if (ImGui.Begin($"##secondaryInv_{entityId:X}", ref open,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
        {
            // Show the entity name in the body rather than the window ID/title, since the name is
            // server-controlled content that must not be interpreted by ImGui as an ID suffix.
            ImGui.Text(name);
            gridRenderer.RenderGrid(entityId, "secondaryInvGrid");
        }

        ImGui.End();
        ImGui.PopStyleVar();

        // If the player closed the window (or it was otherwise dismissed), clear the selection.
        if (!open) stateController.SetSecondaryInventoryEntity(eventSender, 0);
    }
}
