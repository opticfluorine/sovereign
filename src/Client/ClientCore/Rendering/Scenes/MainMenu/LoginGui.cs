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
    private const string LoggingIn = "Logging in...";
    private const string Ok = "OK";

    private const int MaxFieldSize = 256;
    private readonly string errorText = "";
    private LoginState loginState = LoginState.Input;
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
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);

        var nextState = loginState switch
        {
            LoginState.Input => DoInputState(),
            LoginState.Pending => DoPendingState(),
            LoginState.Error => DoErrorState(),
            _ => DoInputState()
        };

        ImGui.End();

        ImGui.PopStyleVar();

        return nextState;
    }

    /// <summary>
    ///     Renders login window contents
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoInputState()
    {
        var nextState = MainMenuState.Login;

        ImGui.Text(Username);
        ImGui.SameLine();
        ImGui.InputText("##username", ref usernameInput, MaxFieldSize);
        ImGui.SetItemDefaultFocus();

        ImGui.Text(Password);
        ImGui.SameLine();
        ImGui.InputText("##password", ref passwordInput, MaxFieldSize, ImGuiInputTextFlags.Password);

        if (ImGui.Button(Login)) DoLogin();
        ImGui.SameLine();
        if (ImGui.Button(Cancel))
        {
            Reset();
            nextState = MainMenuState.Startup;
        }

        return nextState;
    }

    /// <summary>
    ///     Renders login window contents for the login pending state.
    /// </summary>
    /// <returns>Next aain menu state.</returns>
    private MainMenuState DoPendingState()
    {
        ImGui.Text(LoggingIn);
        return MainMenuState.Login;
    }

    private MainMenuState DoErrorState()
    {
        ImGui.Text(errorText);
        if (ImGui.Button(Ok)) loginState = LoginState.Input;
        return MainMenuState.Login;
    }

    private void DoLogin()
    {
        loginState = LoginState.Pending;
    }

    /// <summary>
    ///     Resets dialog state.
    /// </summary>
    private void Reset()
    {
        usernameInput = "";
        passwordInput = "";
    }

    /// <summary>
    ///     Internal login states.
    /// </summary>
    private enum LoginState
    {
        /// <summary>
        ///     Login dialog is accepting input.
        /// </summary>
        Input,

        /// <summary>
        ///     Login is pending.
        /// </summary>
        Pending,

        /// <summary>
        ///     Login dialog is reporting an error.
        /// </summary>
        Error
    }
}