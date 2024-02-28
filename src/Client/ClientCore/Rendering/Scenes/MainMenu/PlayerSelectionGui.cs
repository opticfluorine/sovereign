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
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for player character selection after login.
/// </summary>
public class PlayerSelectionGui
{
    private const string Title = "Select Player";
    private readonly PlayerManagementClient client;
    private string errorMessage = "";
    private Task<Option<ListPlayersResponse, string>>? playerListRequest;
    private PlayerSelectionState selectionState = PlayerSelectionState.Loading;

    public PlayerSelectionGui(PlayerManagementClient client)
    {
        this.client = client;
    }

    /// <summary>
    ///     Initializes the player selection GUI after a main menu state change.
    /// </summary>
    public void Initialize()
    {
        // Start retrieval of characters from server.
        selectionState = PlayerSelectionState.Loading;
        playerListRequest = client.ListPlayersAsync();
    }

    /// <summary>
    ///     Renders the player selection GUI.
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

        var nextState = selectionState switch
        {
            PlayerSelectionState.Loading => DoLoading(),
            _ => DoLoading()
        };

        ImGui.End();
        ImGui.PopStyleVar();
        return nextState;
    }

    /// <summary>
    ///     Renders the loading state.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoLoading()
    {
        if (playerListRequest == null)
        {
            errorMessage = "Error: playerListRequest is null.";
            selectionState = PlayerSelectionState.Error;
            return MainMenuState.PlayerSelection;
        }

        if (playerListRequest.IsFaulted)
        {
            errorMessage = playerListRequest.Exception != null
                ? playerListRequest.Exception.Message
                : "An unknown error occurred.";
            selectionState = PlayerSelectionState.Error;
            return MainMenuState.PlayerSelection;
        }

        if (playerListRequest.IsCompletedSuccessfully)
        {
            var result = playerListRequest.Result;
            if (result.HasFirst)
            {
                // Successfully retrieved list.
                selectionState = PlayerSelectionState.Input;
            }
            else
            {
                selectionState = PlayerSelectionState.Error;
                errorMessage = result.Second;
            }

            return MainMenuState.PlayerSelection;
        }

        // If we get here, the load is still in progress.
        ImGui.Text("Retrieving player list from server...");
        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Internal states for the player selection GUI.
    /// </summary>
    private enum PlayerSelectionState
    {
        /// <summary>
        ///     Initial loading state while the player list is retrieved from the server.
        /// </summary>
        Loading,

        /// <summary>
        ///     Error state if the player list could not be loaded from the server.
        /// </summary>
        Error,

        /// <summary>
        ///     Input state while the player selects a player.
        /// </summary>
        Input
    }
}