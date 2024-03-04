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
///     GUI for player creation.
/// </summary>
public class CreatePlayerGui
{
    private const string Title = "Create Player";

    private const string Create = "Create";

    private const string Cancel = "Cancel";
    private CreatePlayerState createState = CreatePlayerState.Input;

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
            _ => DoInput()
        };

        ImGui.End();
        ImGui.PopStyleVar();
        return nextState;
    }

    private MainMenuState DoInput()
    {
        var nextState = MainMenuState.PlayerCreation;

        if (ImGui.Button(Create))
            ImGui.SameLine();
        if (ImGui.Button(Cancel)) nextState = MainMenuState.Startup;

        return nextState;
    }

    /// <summary>
    ///     Resets the internal state of the GUI.
    /// </summary>
    private void Reset()
    {
        createState = CreatePlayerState.Input;
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