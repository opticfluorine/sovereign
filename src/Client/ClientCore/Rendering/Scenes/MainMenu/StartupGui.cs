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
using Sovereign.ClientCore.Rendering.Configuration;

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
    private readonly DisplayViewport viewport;

    public StartupGui(DisplayViewport viewport)
    {
        this.viewport = viewport;
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

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);
        if (ImGui.Button(Login)) newState = MainMenuState.Login;
        ImGui.Button(Register);
        ImGui.Button(Exit);
        ImGui.End();

        ImGui.PopStyleVar();

        return newState;
    }
}