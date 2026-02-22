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

using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Inventory;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Player;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Manages the in-game GUI.
/// </summary>
public class GameGui
{
    private readonly ChatGui chatGui;
    private readonly DialogueGui dialogueGui;
    private readonly EntityDebugGui entityDebugGui;
    private readonly InventoryGui inventoryGui;
    private readonly ItemContextGui itemContextGui;
    private readonly InGameMenuGui menuGui;
    private readonly OverlayGui overlayGui;
    private readonly PlayerDebugGui playerDebugGui;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ClientStateServices stateServices;
    private readonly TemplateEditorGui templateEditorGui;
    private readonly WorldEditorGui worldEditorGui;

    public GameGui(ClientStateServices stateServices, PlayerDebugGui playerDebugGui, EntityDebugGui entityDebugGui,
        InGameMenuGui menuGui, ChatGui chatGui, TemplateEditorGui templateEditorGui, PlayerRoleCheck roleCheck,
        WorldEditorGui worldEditorGui, OverlayGui overlayGui, DialogueGui dialogueGui, InventoryGui inventoryGui,
        ItemContextGui itemContextGui)
    {
        this.stateServices = stateServices;
        this.playerDebugGui = playerDebugGui;
        this.entityDebugGui = entityDebugGui;
        this.menuGui = menuGui;
        this.chatGui = chatGui;
        this.templateEditorGui = templateEditorGui;
        this.roleCheck = roleCheck;
        this.worldEditorGui = worldEditorGui;
        this.overlayGui = overlayGui;
        this.dialogueGui = dialogueGui;
        this.inventoryGui = inventoryGui;
        this.itemContextGui = itemContextGui;
    }

    /// <summary>
    ///     Renders the in-game GUI.
    /// </summary>
    /// <param name="renderPlan">Render plan for the current frame.</param>
    public unsafe void Render(RenderPlan renderPlan)
    {
        UpdateDebugGui();
        UpdateAdminGui();

        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowInGameMenu)) menuGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowChat)) chatGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowInventory)) inventoryGui.Render();

        overlayGui.Render(renderPlan);
        dialogueGui.Render();
        itemContextGui.Render();

        // Clear window focus when first entering the game so that controls aren't absorbed by the GUI.
        if (stateServices.CheckAndClearFlagValue(ClientStateFlag.NewLogin))
            ImGui.SetWindowFocus((byte*)null);
    }

    /// <summary>
    ///     Renders any open in-game debug windows.
    /// </summary>
    private void UpdateDebugGui()
    {
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowPlayerDebug)) playerDebugGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowEntityDebug)) entityDebugGui.Render();
    }

    /// <summary>
    ///     Conditionally renders any admin-only windows.
    /// </summary>
    private void UpdateAdminGui()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerEntityId) ||
            !roleCheck.IsPlayerAdmin(playerEntityId)) return;

        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowTemplateEntityEditor)) templateEditorGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.WorldEditMode)) worldEditorGui.Render();
    }
}