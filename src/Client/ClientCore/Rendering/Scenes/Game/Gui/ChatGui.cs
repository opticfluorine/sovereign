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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     GUI for chat.
/// </summary>
public class ChatGui
{
    private static readonly Vector2 WindowRelPos = new(0.15f, 0.75f);
    private static readonly Vector2 TableScale = new(0.7f, 0.2f);
    private string input = "";

    /// <summary>
    ///     Renders the chat window.
    /// </summary>
    public void Render()
    {
        var io = ImGui.GetIO();
        var chatPos = io.DisplaySize * WindowRelPos;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGui.SetNextWindowSize(Vector2.Zero);
        ImGui.SetNextWindowPos(chatPos, ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Chat", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar))
        {
            var tableDims = io.DisplaySize * TableScale;
            if (ImGui.BeginTable("chat", 1, ImGuiTableFlags.ScrollY, tableDims)) ImGui.EndTable();

            ImGui.PushItemWidth(tableDims.X);
            if (ImGui.InputText("##chatInput", ref input, 128,
                    ImGuiInputTextFlags.EnterReturnsTrue)) OnSubmit();
            ImGui.PopItemWidth();
            ImGui.End();
        }

        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Called when the player submits a chat message.
    /// </summary>
    private void OnSubmit()
    {
        // Ignore zero-length inputs.
        if (input.Length == 0) return;
    }
}