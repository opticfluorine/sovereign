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
    ///     Flag indicating that the in-game menu should be displayed.
    /// </summary>
    ShowInGameMenu,

    /// <summary>
    ///     Flag indicating that the in-game player debug window should be displayed.
    /// </summary>
    ShowPlayerDebug,

    /// <summary>
    ///     Flag indicating that the in-game entity debug window should be displayed.
    /// </summary>
    ShowEntityDebug
}