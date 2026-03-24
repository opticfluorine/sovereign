// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Hexa.NET.ImGui;
using Sovereign.ClientCore.Systems.ClientState;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;

/// <summary>
///     Debug GUI for renderer debugging.
/// </summary>
public sealed class RendererDebugGui(DebugState debugState)
{
    /// <summary>
    ///     Renders the renderer debug GUI.
    /// </summary>
    public void Render()
    {
        if (!ImGui.Begin("Renderer Debug", ImGuiWindowFlags.AlwaysAutoResize)) return;
        var fontSize = ImGui.GetFontSize();

        var enableZCap = debugState.EnableZLayerCap;
        ImGui.Checkbox("Enable Z Layer Cap", ref enableZCap);
        debugState.EnableZLayerCap = enableZCap;

        var zCapOffset = debugState.ZLayerCapOffset;
        ImGui.Text("Z Layer Cap Offset:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(fontSize * 4.0f);
        ImGui.InputInt("##zOff", ref zCapOffset);
        debugState.ZLayerCapOffset = zCapOffset;

        ImGui.End();
    }
}