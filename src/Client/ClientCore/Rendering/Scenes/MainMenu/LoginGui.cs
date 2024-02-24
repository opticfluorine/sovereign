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

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for login.
/// </summary>
public class LoginGui
{
    private const string Title = "Login";
    private const string Username = "Username";
    private const string Password = "Password";
    private const string Login = "Login";
    private const string Cancel = "Cancel";

    private const int MaxFieldSize = 256;
    private string passwordInput = "";

    private string usernameInput = "";

    /// <summary>
    ///     Renders the login dialog.
    /// </summary>
    /// <returns>
    ///     Next state.
    /// </returns>
    public MainMenuState Render()
    {
        var nextState = MainMenuState.Login;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);

        ImGui.Text(Username);
        ImGui.SameLine();
        ImGui.InputText("##username", ref usernameInput, MaxFieldSize);

        ImGui.Text(Password);
        ImGui.SameLine();
        ImGui.InputText("##password", ref passwordInput, MaxFieldSize, ImGuiInputTextFlags.Password);

        ImGui.Button(Login);
        ImGui.SameLine();
        if (ImGui.Button(Cancel))
        {
            Reset();
            nextState = MainMenuState.Startup;
        }

        ImGui.End();

        ImGui.PopStyleVar();

        return nextState;
    }

    /// <summary>
    ///     Resets dialog state.
    /// </summary>
    private void Reset()
    {
        usernameInput = "";
        passwordInput = "";
    }
}