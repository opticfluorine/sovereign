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
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;

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
    private readonly ClientNetworkController clientNetworkController;
    private readonly ClientConfigurationManager configManager;
    private readonly IEventSender eventSender;
    private readonly ClientNetworkManager networkManager;
    private string errorText = "";
    private LoginState loginState = LoginState.Input;
    private string passwordInput = "";
    private string usernameInput = "";

    public LoginGui(ClientNetworkManager networkManager, IEventSender eventSender,
        ClientNetworkController clientNetworkController, ClientConfigurationManager configManager)
    {
        this.networkManager = networkManager;
        this.eventSender = eventSender;
        this.clientNetworkController = clientNetworkController;
        this.configManager = configManager;
    }

    /// <summary>
    ///     Initializes the GUI on main menu state change.
    /// </summary>
    public void Initialize()
    {
        Reset();
    }

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
        if (ImGui.Button(Cancel)) nextState = MainMenuState.Startup;

        return nextState;
    }

    /// <summary>
    ///     Renders login window contents for the login pending state.
    /// </summary>
    /// <returns>Next aain menu state.</returns>
    private MainMenuState DoPendingState()
    {
        ImGui.Text(LoggingIn);

        if (networkManager.ClientState == NetworkClientState.Connected)
            // Success.
            return MainMenuState.PlayerSelection;

        if (networkManager.ClientState == NetworkClientState.Failed)
        {
            // Error.
            errorText = networkManager.ErrorMessage;
            loginState = LoginState.Error;
        }

        return MainMenuState.Login;
    }

    /// <summary>
    ///     Renders error message.
    /// </summary>
    /// <returns></returns>
    private MainMenuState DoErrorState()
    {
        ImGui.Text(errorText);
        if (ImGui.Button(Ok)) loginState = LoginState.Input;
        return MainMenuState.Login;
    }

    /// <summary>
    ///     Initiates a login.
    /// </summary>
    private void DoLogin()
    {
        // Validate input.
        if (usernameInput.Length == 0)
        {
            errorText = "Username must not be empty.";
            loginState = LoginState.Error;
            return;
        }

        if (passwordInput.Length == 0)
        {
            errorText = "Password must not be empty.";
            loginState = LoginState.Error;
            return;
        }

        // Initiate login.
        loginState = LoginState.Pending;
        clientNetworkController.BeginConnection(eventSender, configManager.ClientConfiguration.ConnectionParameters,
            new LoginParameters(usernameInput, passwordInput));
    }

    /// <summary>
    ///     Resets dialog state.
    /// </summary>
    private void Reset()
    {
        loginState = LoginState.Input;
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