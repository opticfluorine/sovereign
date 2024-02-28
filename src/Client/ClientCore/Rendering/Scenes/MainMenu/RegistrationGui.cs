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
using System.Threading.Tasks;
using Castle.Core.Logging;
using ImGuiNET;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for registration.
/// </summary>
public class RegistrationGui
{
    private const string Title = "Register";
    private const string Username = "Username";
    private const string Password = "Password";
    private const string ConfirmPassword = "Confirm Password";
    private const string Register = "Register";
    private const string Cancel = "Cancel";

    /// <summary>
    ///     Maximum field length.
    /// </summary>
    private const int MaxFieldSize = 256;

    private const string Ok = "OK";

    private const string Registering = "Registering...";
    private const string UnknownError = "An unknown error occurred during registration.";

    private const string Successful = "Registration successful.";

    private readonly ClientConfigurationManager configManager;

    private readonly RegistrationClient registrationClient;

    private string confirmPasswordInput = "";
    private string errorMessage = "";
    private string passwordInput = "";

    /// <summary>
    ///     Current registration state.
    /// </summary>
    private RegistrationState registrationState = RegistrationState.Input;

    /// <summary>
    ///     Registration background task.
    /// </summary>
    private Task<Option<RegistrationResponse, string>>? registrationTask;

    private string usernameInput = "";

    public RegistrationGui(RegistrationClient registrationClient, ClientConfigurationManager configManager)
    {
        this.registrationClient = registrationClient;
        this.configManager = configManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Initializes the registration GUI on main menu state change.
    /// </summary>
    public void Initialize()
    {
        Reset();
    }

    /// <summary>
    ///     Renders the registration dialog.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    public MainMenuState Render()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);
        var nextState = registrationState switch
        {
            RegistrationState.Input => DoInputState(),
            RegistrationState.Pending => OnPendingState(),
            RegistrationState.Success => OnSuccessState(),
            RegistrationState.Error => DoErrorState(),
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
        var nextState = MainMenuState.Registration;

        if (ImGui.BeginTable("register", 2))
        {
            ImGui.TableNextColumn();
            ImGui.Text(Username);
            ImGui.TableNextColumn();
            ImGui.PushItemWidth(-4.0f);
            ImGui.InputText("##username", ref usernameInput, MaxFieldSize);
            ImGui.SetItemDefaultFocus();
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.Text(Password);
            ImGui.TableNextColumn();
            ImGui.InputText("##password", ref passwordInput, MaxFieldSize, ImGuiInputTextFlags.Password);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.Text(ConfirmPassword);
            ImGui.TableNextColumn();
            ImGui.InputText("##confirm", ref confirmPasswordInput, MaxFieldSize, ImGuiInputTextFlags.Password);

            ImGui.PopItemWidth();
            ImGui.EndTable();
        }

        ImGui.Spacing();
        if (ImGui.Button(Register)) DoRegister();
        ImGui.SameLine();
        if (ImGui.Button(Cancel))
        {
            Reset();
            nextState = MainMenuState.Startup;
        }

        return nextState;
    }

    /// <summary>
    ///     Makes an account registration request.
    /// </summary>
    private void DoRegister()
    {
        // Validation.
        if (usernameInput.Length == 0)
        {
            errorMessage = "Username must not be empty.";
            registrationState = RegistrationState.Error;
            return;
        }

        if (passwordInput.Length == 0)
        {
            errorMessage = "Passowrd must not be empty.";
            registrationState = RegistrationState.Error;
            return;
        }

        if (!passwordInput.Equals(confirmPasswordInput))
        {
            errorMessage = "Passwords do not match.";
            registrationState = RegistrationState.Error;
            return;
        }

        // Send request.
        var request = new RegistrationRequest
        {
            Username = usernameInput,
            Password = passwordInput
        };
        registrationTask =
            registrationClient.RegisterAsync(configManager.ClientConfiguration.ConnectionParameters, request);
        registrationState = RegistrationState.Pending;
    }

    /// <summary>
    ///     Renders the GUI for the pending state.
    /// </summary>
    /// <returns></returns>
    /// d
    private MainMenuState OnPendingState()
    {
        const MainMenuState nextState = MainMenuState.Registration;
        ImGui.Text(Registering);

        if (registrationTask == null)
        {
            errorMessage = UnknownError;
            registrationState = RegistrationState.Error;
            return nextState;
        }

        if (registrationTask.IsCompletedSuccessfully)
        {
            var result = registrationTask.Result;
            if (result.HasFirst)
            {
                // Successful registration.
                registrationState = RegistrationState.Success;
            }
            else
            {
                // Error.
                errorMessage = result.Second;
                registrationState = RegistrationState.Error;
            }
        }

        return nextState;
    }

    /// <summary>
    ///     Renders the registration dialog contents in the success state.
    /// </summary>
    /// <returns>Next state.</returns>
    private MainMenuState OnSuccessState()
    {
        ImGui.Text(Successful);
        if (ImGui.Button(Ok)) return MainMenuState.Login;
        return MainMenuState.Registration;
    }

    /// <summary>
    ///     Renders the registration dialog contents in the error state.
    /// </summary>
    /// <returns>Next state.</returns>
    private MainMenuState DoErrorState()
    {
        ImGui.Text(errorMessage);
        if (ImGui.Button(Ok)) registrationState = RegistrationState.Input;
        return MainMenuState.Registration;
    }

    /// <summary>
    ///     Resets dialog state.
    /// </summary>
    private void Reset()
    {
        usernameInput = "";
        passwordInput = "";
        confirmPasswordInput = "";
    }

    private enum RegistrationState
    {
        Input,
        Pending,
        Success,
        Error
    }
}