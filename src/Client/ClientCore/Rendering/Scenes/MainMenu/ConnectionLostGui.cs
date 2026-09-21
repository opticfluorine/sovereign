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

using System;
using System.Numerics;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Systems.ClientState;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     GUI for the connection lost error dialog.
/// </summary>
public class ConnectionLostGui
{
    private readonly AutoLoginOptions autoLoginOptions;
    private readonly ILogger<ConnectionLostGui> logger;

    /// <summary>
    ///     Initializes the connection lost dialog.
    /// </summary>
    /// <param name="autoLoginOptions">Automatic login options.</param>
    /// <param name="logger">Logger.</param>
    public ConnectionLostGui(IOptions<AutoLoginOptions> autoLoginOptions, ILogger<ConnectionLostGui> logger)
    {
        this.autoLoginOptions = autoLoginOptions.Value;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the GUI for the connection lost error dialog.
    /// </summary>
    /// <returns>Next main menu state.</returns>
    public MainMenuState Render()
    {
        if (autoLoginOptions.Enabled)
        {
            logger.LogError("Connection to server lost while automatic login is enabled.");
            Environment.Exit(1);
        }

        var nextState = MainMenuState.ConnectionLost;

        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(0.5f * io.DisplaySize, ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
        ImGui.Begin("Error");
        ImGui.Text("Connection to server lost.");
        if (ImGui.Button("OK")) nextState = MainMenuState.Startup;
        ImGui.End();

        return nextState;
    }
}