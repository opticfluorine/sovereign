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
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     Initial GUI displayed at startup.
/// </summary>
public class StartupGui
{
    private const string Title = "Sovereign Engine";
    private const string Login = "Login";
    private const string Register = "Register";
    private const string Exit = "Exit";

    private static readonly Vector2 ButtonSize = new(7.11f, 1.33f);
    private readonly CoreController coreController;
    private readonly IEventSender eventSender;

    public StartupGui(IEventSender eventSender, CoreController coreController)
    {
        this.eventSender = eventSender;
        this.coreController = coreController;
    }

    /// <summary>
    ///     Renders the startup GUI.
    /// </summary>
    /// <returns>
    ///     Main menu state after processing inputs.
    /// </returns>
    public MainMenuState Render()
    {
        var newState = MainMenuState.Startup;

        var fontSize = ImGui.GetFontSize();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, fontSize * new Vector2(0.8f, 0.8f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(fontSize * new Vector2(8.89f, 0.0f), ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);

        if (ImGui.BeginTable("startup", 1))
        {
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("##startupCol", ImGuiTableColumnFlags.WidthStretch);
            if (ImGui.Button(Login, fontSize * ButtonSize)) newState = MainMenuState.Login;

            ImGui.TableNextColumn();
            if (ImGui.Button(Register, fontSize * ButtonSize)) newState = MainMenuState.Registration;

            ImGui.TableNextColumn();
            if (ImGui.Button(Exit, fontSize * ButtonSize)) coreController.Quit(eventSender);

            ImGui.EndTable();
        }

        ImGui.End();

        ImGui.PopStyleVar();

        return newState;
    }
}