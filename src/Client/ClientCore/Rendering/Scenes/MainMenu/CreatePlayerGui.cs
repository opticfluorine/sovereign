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
using ImGuiNET;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for player creation.
/// </summary>
public class CreatePlayerGui
{
    private const string Title = "Create Player";

    private const string Create = "Create";

    private const string Cancel = "Cancel";

    private const string Ok = "Ok";

    private const string CreatingPlayer = "Creating player and entering world...";

    private readonly PlayerManagementClient client;
    private CreatePlayerState createState = CreatePlayerState.Input;
    private Task<Option<CreatePlayerResponse, string>>? createTask;

    private string errorMessage = "";
    private string playerNameInput = "";
    private bool setDefaultFocus = true;

    public CreatePlayerGui(PlayerManagementClient client)
    {
        this.client = client;
    }

    /// <summary>
    ///     Initializes the GUI.
    /// </summary>
    public void Initialize()
    {
        Reset();
    }

    /// <summary>
    ///     Renders the GUI.
    /// </summary>
    /// <returns></returns>
    public MainMenuState Render()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);

        var nextState = createState switch
        {
            CreatePlayerState.Input => DoInput(),
            CreatePlayerState.Pending => DoPending(),
            CreatePlayerState.Error => DoError(),
            _ => DoInput()
        };

        ImGui.End();
        ImGui.PopStyleVar();
        return nextState;
    }

    /// <summary>
    ///     Renders the GUI for the input state.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoInput()
    {
        var nextState = MainMenuState.PlayerCreation;

        ImGui.BeginTable("createPlayer", 2, ImGuiTableFlags.SizingFixedFit);

        ImGui.TableNextColumn();
        ImGui.Text("Player name:");
        ImGui.TableNextColumn();
        if (setDefaultFocus)
        {
            ImGui.SetKeyboardFocusHere();
            setDefaultFocus = false;
        }

        ImGui.SetNextItemWidth(240.0f);
        if (ImGui.InputText("##playerName", ref playerNameInput, EntityConstants.MaxNameLength,
                ImGuiInputTextFlags.EnterReturnsTrue)) OnCreate();

        ImGui.EndTable();

        if (ImGui.Button(Create)) OnCreate();
        ImGui.SameLine();
        if (ImGui.Button(Cancel)) nextState = MainMenuState.PlayerSelection;

        return nextState;
    }

    /// <summary>
    ///     Renders the GUI for the pending state.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoPending()
    {
        if (createTask == null)
        {
            OnError("createTask is null.");
        }
        else if (createTask.IsFaulted)
        {
            if (createTask.Exception == null) OnError("Unknown failure.");
            else OnError(createTask.Exception.Message);
        }
        else if (createTask.IsCompletedSuccessfully && createTask.Result.HasSecond)
        {
            OnError(createTask.Result.Second);
        }
        else
        {
            ImGui.Text(CreatingPlayer);
        }

        return MainMenuState.PlayerCreation;
    }

    /// <summary>
    ///     Renders an error message.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoError()
    {
        ImGui.Text(errorMessage);
        if (ImGui.Button(Ok)) createState = CreatePlayerState.Input;

        return MainMenuState.PlayerCreation;
    }

    /// <summary>
    ///     Creates the player when the create button is clicked.
    /// </summary>
    private void OnCreate()
    {
        // Validate.
        if (playerNameInput.Length == 0)
        {
            OnError("Player name is required.");
            return;
        }

        // Attempt to create player.
        var request = new CreatePlayerRequest
        {
            PlayerName = playerNameInput
        };
        createTask = client.CreatePlayerAsync(request);
        createState = CreatePlayerState.Pending;
    }

    /// <summary>
    ///     Displays an error to the user.
    /// </summary>
    /// <param name="message">Error message.</param>
    private void OnError(string message)
    {
        errorMessage = message;
        createState = CreatePlayerState.Error;
    }

    /// <summary>
    ///     Resets the internal state of the GUI.
    /// </summary>
    private void Reset()
    {
        createState = CreatePlayerState.Input;
        setDefaultFocus = true;
    }

    /// <summary>
    ///     Internal player creation state.
    /// </summary>
    private enum CreatePlayerState
    {
        /// <summary>
        ///     Player input state.
        /// </summary>
        Input,

        /// <summary>
        ///     Creation pending state.
        /// </summary>
        Pending,

        /// <summary>
        ///     Error state.
        /// </summary>
        Error
    }
}