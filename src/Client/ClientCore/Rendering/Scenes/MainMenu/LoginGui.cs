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
using Hexa.NET.ImGui;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Rendering.Gui;
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
    private readonly ConnectionOptions connectionOptions;
    private readonly IEventSender eventSender;
    private readonly ClientNetworkManager networkManager;
    private string errorText = "";
    private LoginState loginState = LoginState.Input;
    private string passwordInput = "";
    private bool setDefaultFocus = true;
    private string usernameInput = "";

    public LoginGui(ClientNetworkManager networkManager, IEventSender eventSender,
        ClientNetworkController clientNetworkController,
        IOptions<ConnectionOptions> connectionOptions)
    {
        this.networkManager = networkManager;
        this.eventSender = eventSender;
        this.clientNetworkController = clientNetworkController;
        this.connectionOptions = connectionOptions.Value;
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
        var fontSize = ImGui.GetFontSize();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, fontSize * new Vector2(0.8f, 0.8f));

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
        var fontSize = ImGui.GetFontSize();

        if (!ImGui.BeginTable("login", 2, ImGuiTableFlags.SizingFixedFit)) return nextState;

        ImGui.TableNextColumn();
        ImGui.Text(Username);

        ImGui.TableNextColumn();
        ImGui.PushItemWidth(fontSize * 8.0f);
        if (setDefaultFocus)
        {
            ImGui.SetKeyboardFocusHere();
            setDefaultFocus = false;
        }

        if (GuiWorkarounds.InputTextEnterReturns("##username"u8, ref usernameInput, MaxFieldSize)) DoLogin();

        ImGui.SetItemDefaultFocus();

        ImGui.TableNextColumn();
        ImGui.Text(Password);

        ImGui.TableNextColumn();
        if (GuiWorkarounds.InputTextEnterReturns("##password"u8, ref passwordInput, MaxFieldSize,
                ImGuiInputTextFlags.Password)) DoLogin();

        ImGui.PopItemWidth();
        ImGui.EndTable();

        ImGui.Spacing();
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
        clientNetworkController.BeginConnection(eventSender, connectionOptions,
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
        setDefaultFocus = true;
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