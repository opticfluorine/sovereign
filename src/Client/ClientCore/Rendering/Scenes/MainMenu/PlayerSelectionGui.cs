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
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for player character selection after login.
/// </summary>
public class PlayerSelectionGui
{
    private const string Title = "Select Player";

    private const string Ok = "OK";

    private const string CreatePlayer = "Create New Player";

    private const string Logout = "Logout";

    private const string Yes = "Yes";
    private const string No = "No";

    private const string ConfirmDelete = "Confirm Player Delete";

    private const string EnteringWorld = "Entering world...";
    private readonly PlayerManagementClient client;
    private readonly IEventSender eventSender;
    private readonly GuiExtensions guiExtensions;
    private readonly ClientNetworkController networkController;
    private Task<Option<DeletePlayerResponse, string>>? deletionTask;
    private string errorMessage = "";
    private Task<Option<ListPlayersResponse, string>>? playerListRequest;

    /// <summary>
    ///     If not null, specifies the player that is pending deletion.
    /// </summary>
    private PlayerInfo? playerToDelete;

    private PlayerSelectionState selectionState = PlayerSelectionState.Loading;
    private Task<Option<SelectPlayerResponse, string>>? selectionTask;

    public PlayerSelectionGui(PlayerManagementClient client, ClientNetworkController networkController,
        IEventSender eventSender, GuiExtensions guiExtensions)
    {
        this.client = client;
        this.networkController = networkController;
        this.eventSender = eventSender;
        this.guiExtensions = guiExtensions;
    }

    /// <summary>
    ///     Initializes the player selection GUI after a main menu state change.
    /// </summary>
    public void Initialize()
    {
        LoadSelections();
    }

    /// <summary>
    ///     Renders the player selection GUI.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    public MainMenuState Render()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));

        var windowSize = selectionState == PlayerSelectionState.Input ? new Vector2(0.0f, 520.0f) : Vector2.Zero;

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin(Title);

        var nextState = selectionState switch
        {
            PlayerSelectionState.Loading => DoLoading(),
            PlayerSelectionState.Error => DoError(),
            PlayerSelectionState.Input => DoInput(),
            PlayerSelectionState.Selected => DoSelected(),
            PlayerSelectionState.Deleting => DoDeleting(),
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
        }
        else if (playerListRequest.IsFaulted)
        {
            errorMessage = playerListRequest.Exception != null
                ? playerListRequest.Exception.Message
                : "An unknown error occurred.";
            selectionState = PlayerSelectionState.Error;
        }
        else if (playerListRequest.IsCompletedSuccessfully)
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
        }

        // If we get here, the load is still in progress.
        ImGui.Text("Retrieving player list from server...");
        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Renders the input state.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoInput()
    {
        if (playerListRequest == null)
        {
            errorMessage = "Player list request is null.";
            selectionState = PlayerSelectionState.Error;
            return MainMenuState.PlayerSelection;
        }

        var playerList = playerListRequest.Result.First.Players;
        if (playerList == null)
        {
            errorMessage = "Missing response from server.";
            selectionState = PlayerSelectionState.Error;
            return MainMenuState.PlayerSelection;
        }

        ImGui.BeginTable("players", 3,
            ImGuiTableFlags.BordersH | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit,
            new Vector2(0.0f, 440.0f));
        foreach (var player in playerList) RenderPlayer(player);

        ImGui.EndTable();

        if (ImGui.Button(CreatePlayer)) return MainMenuState.PlayerCreation;
        ImGui.SameLine();
        if (ImGui.Button(Logout))
        {
            networkController.EndConnection(eventSender);
            return MainMenuState.Startup;
        }


        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Renders a single player character into a row of the player character table.
    /// </summary>
    /// <param name="player">Player.</param>
    private void RenderPlayer(PlayerInfo player)
    {
        ImGui.TableNextColumn();
        if (player.AnimatedSprite.HasValue)
            guiExtensions.AnimatedSprite(player.AnimatedSprite.Value, Orientation.South, AnimationPhase.Moving);

        ImGui.TableNextColumn();
        ImGui.Text(player.Name);

        ImGui.TableNextColumn();
        if (ImGui.Button(new StringBuilder("Play##").Append(player.Id).ToString())) OnSelect(player);
        ImGui.SameLine();
        if (ImGui.Button(new StringBuilder("Delete##").Append(player.Id).ToString()))
        {
            playerToDelete = player;
            ImGui.OpenPopup(ConfirmDelete);
        }

        if (playerToDelete != null && ImGui.BeginPopupModal(ConfirmDelete))
        {
            var message = new StringBuilder("Are you sure that you want to permanently delete player ")
                .Append(playerToDelete.Name).Append('?').ToString();
            ImGui.Text(message);
            if (ImGui.Button(Yes))
            {
                OnDelete(playerToDelete);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(No))
            {
                playerToDelete = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Handles a player selection.
    /// </summary>
    /// <param name="player">Selected player.</param>
    private void OnSelect(PlayerInfo player)
    {
        selectionTask = client.SelectPlayerAsync(player.Id);
        selectionState = PlayerSelectionState.Selected;
    }

    /// <summary>
    ///     Handles a player deletion.
    /// </summary>
    /// <param name="player">Player to delete.</param>
    private void OnDelete(PlayerInfo player)
    {
        deletionTask = client.DeletePlayerAsync(player.Id);
        selectionState = PlayerSelectionState.Deleting;
    }

    /// <summary>
    ///     Renders an error message.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    private MainMenuState DoError()
    {
        ImGui.Text(errorMessage);
        if (ImGui.Button(Ok))
        {
            // Full logout.
            networkController.EndConnection(eventSender);
            return MainMenuState.Startup;
        }

        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Renders the GUI for the Selected state while waiting for world entry.
    /// </summary>
    /// <returns></returns>
    private MainMenuState DoSelected()
    {
        if (selectionTask == null)
        {
            errorMessage = "selectionTask is null.";
            selectionState = PlayerSelectionState.Error;
        }
        else if (selectionTask.IsCompletedSuccessfully)
        {
            if (selectionTask.Result.HasSecond)
            {
                ImGui.Text(selectionTask.Result.Second);
                if (ImGui.Button(Ok)) LoadSelections();
            }
            else
            {
                ImGui.Text(EnteringWorld);
            }
        }
        else if (selectionTask.IsFaulted)
        {
            if (selectionTask.Exception != null)
            {
                var message = new StringBuilder("Error: ").Append(selectionTask.Exception.Message).ToString();
                ImGui.Text(message);
                if (ImGui.Button(Ok)) LoadSelections();
            }
        }
        else
        {
            // This is the final state of the selection flow in the GUI.
            // Once the client is notified that it has entered the world, it will automatically
            // transition to the in-game state.
            ImGui.Text(EnteringWorld);
        }

        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Renders the GUI while deletion is in progress.
    /// </summary>
    /// <returns></returns>
    private MainMenuState DoDeleting()
    {
        if (deletionTask == null)
        {
            errorMessage = "deletionTask is null.";
            selectionState = PlayerSelectionState.Error;
        }
        else if (deletionTask.IsCompletedSuccessfully)
        {
            if (deletionTask.Result.HasSecond)
            {
                errorMessage = deletionTask.Result.Second;
                selectionState = PlayerSelectionState.Error;
            }
            else
            {
                LoadSelections();
            }
        }
        else if (deletionTask.IsFaulted)
        {
            errorMessage = deletionTask.Exception == null ? "Unknown error." : deletionTask.Exception.Message;
            selectionState = PlayerSelectionState.Error;
        }
        else
        {
            ImGui.Text("Deleting player...");
        }

        return MainMenuState.PlayerSelection;
    }

    /// <summary>
    ///     Asynchronously loads selectable players.
    /// </summary>
    private void LoadSelections()
    {
        selectionState = PlayerSelectionState.Loading;
        playerListRequest = client.ListPlayersAsync();
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
        Input,

        /// <summary>
        ///     Player selected and world entry is in progress.
        /// </summary>
        Selected,

        /// <summary>
        ///     Player deletion in progress.
        /// </summary>
        Deleting
    }
}