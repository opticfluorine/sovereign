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

using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;
using Sovereign.ClientCore.Systems.ClientState;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Manages the in-game GUI.
/// </summary>
public class GameGui
{
    private readonly ChatGui chatGui;
    private readonly EntityDebugGui entityDebugGui;
    private readonly InGameMenuGui menuGui;
    private readonly PlayerDebugGui playerDebugGui;
    private readonly ClientStateServices stateServices;

    public GameGui(ClientStateServices stateServices, PlayerDebugGui playerDebugGui, EntityDebugGui entityDebugGui,
        InGameMenuGui menuGui, ChatGui chatGui)
    {
        this.stateServices = stateServices;
        this.playerDebugGui = playerDebugGui;
        this.entityDebugGui = entityDebugGui;
        this.menuGui = menuGui;
        this.chatGui = chatGui;
    }

    /// <summary>
    ///     Renders the in-game GUI.
    /// </summary>
    public void Render()
    {
        UpdateDebugGui();

        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowInGameMenu)) menuGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowChat)) chatGui.Render();
    }

    /// <summary>
    ///     Renders any open in-game debug windows.
    /// </summary>
    private void UpdateDebugGui()
    {
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowPlayerDebug)) playerDebugGui.Render();
        if (stateServices.GetStateFlagValue(ClientStateFlag.ShowEntityDebug)) entityDebugGui.Render();
    }
}