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

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
/// </summary>
public enum ClientStateFlag
{
    /// <summary>
    ///     Flag indicating that the Dear ImGui metrics window should be displayed.
    /// </summary>
    ShowImGuiMetrics,

    /// <summary>
    ///     Flag indicating that the Dear ImGui debug log window should be displayed.
    /// </summary>
    ShowImGuiDebugLog,

    /// <summary>
    ///     Flag indicating that the Dear ImGui ID stack tool window should be displayed.
    /// </summary>
    ShowImGuiIdStackTool,

    /// <summary>
    ///     Flag indicating that the Dear ImGui demo window should be displayed.
    /// </summary>
    ShowImGuiDemo,

    /// <summary>
    ///     Flag indicating that the network debug window should be displayed.
    /// </summary>
    ShowNetworkDebug,

    /// <summary>
    ///     Flag indicating that the in-game menu should be displayed.
    /// </summary>
    ShowInGameMenu,

    /// <summary>
    ///     Flag indicating that the chat window should be displayed.
    /// </summary>
    ShowChat,

    /// <summary>
    ///     Flag indicating that the in-game player debug window should be displayed.
    /// </summary>
    ShowPlayerDebug,

    /// <summary>
    ///     Flag indicating that the in-game entity debug window should be displayed.
    /// </summary>
    ShowEntityDebug,

    /// <summary>
    ///     Flag indicating that the in-game resource editor window should be displayed.
    /// </summary>
    ShowResourceEditor,

    /// <summary>
    ///     Flag indicating that the in-game template entity editor window should be displayed.
    /// </summary>
    ShowTemplateEntityEditor,

    /// <summary>
    ///     Flag indicating that the inventory GUI should be displayed.
    /// </summary>
    ShowInventory,

    /// <summary>
    ///     Flag indicating that the game is in world edit mode.
    /// </summary>
    WorldEditMode,

    /// <summary>
    ///     Flag indicating the need to reload client resources (e.g. after an update).
    /// </summary>
    ReloadClientResources,

    /// <summary>
    ///     Flag indicating that the player has newly logged into the game in the last frame.
    /// </summary>
    NewLogin,

    /// <summary>
    ///     Flag indicating that the next frame should be traced for debugging.
    /// </summary>
    DebugFrame,

    /// <summary>
    ///     Flag indicating that hidden entities should be drawn with a placeholder.
    /// </summary>
    ShowHiddenEntities
}