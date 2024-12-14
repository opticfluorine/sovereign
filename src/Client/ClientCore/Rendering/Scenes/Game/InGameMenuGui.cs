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
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering.Scenes.Game;

/// <summary>
///     Provides the in-game menu GUI.
/// </summary>
public class InGameMenuGui
{
    private static readonly Vector2 ButtonSize = new(7.11f, 1.33f);
    private readonly ClientNetworkController clientNetworkController;
    private readonly CoreController coreController;
    private readonly IEventSender eventSender;

    public InGameMenuGui(IEventSender eventSender, ClientNetworkController clientNetworkController,
        CoreController coreController)
    {
        this.eventSender = eventSender;
        this.clientNetworkController = clientNetworkController;
        this.coreController = coreController;
    }

    /// <summary>
    ///     Renders the in-game menu.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, fontSize * new Vector2(0.8f, 0.8f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(fontSize * new Vector2(8.89f, 0.0f), ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin("##inGameMenu",
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

        if (ImGui.BeginTable("gameMenu", 1))
        {
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("##gameMenuCol", ImGuiTableColumnFlags.WidthStretch);
            if (ImGui.Button("Logout", fontSize * ButtonSize)) OnLogout();

            ImGui.TableNextColumn();
            if (ImGui.Button("Exit", fontSize * ButtonSize)) OnExit();

            ImGui.EndTable();
        }

        ImGui.End();

        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Called when the player clicks the Exit button.
    /// </summary>
    private void OnExit()
    {
        coreController.Quit(eventSender);
    }

    /// <summary>
    ///     Called when the user clicks the Logout button.
    /// </summary>
    private void OnLogout()
    {
        clientNetworkController.LogoutPlayer(eventSender);
    }
}