// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using Sovereign.ClientCore.Network.Infrastructure;

namespace Sovereign.ClientCore.Rendering.Gui.Debug;

/// <summary>
///     Network debug window.
/// </summary>
public class NetworkDebugGui(ClientNetworkManager networkManager)
{
    /// <summary>
    ///     Renders the network debug window.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(fontSize * new Vector2(16.0f, 9.0f), ImGuiCond.Once);
        if (!ImGui.Begin("Network Debug")) return;

        if (ImGui.BeginTable("networkStats", 2))
        {
            ImGui.TableNextColumn();
            ImGui.Text("Data Sent:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.NetStatistics.BytesSent} bytes");
            
            ImGui.TableNextColumn();
            ImGui.Text("Data Received:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.NetStatistics.BytesReceived} bytes");

            ImGui.TableNextColumn();
            ImGui.Text("Packets Sent:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.NetStatistics.PacketsSent}");
            
            ImGui.TableNextColumn();
            ImGui.Text("Packets Received:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.NetStatistics.PacketsReceived}");
            
            ImGui.TableNextColumn();
            ImGui.Text("Packet Loss:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.NetStatistics.PacketLoss} ({networkManager.NetStatistics.PacketLossPercent}%)");

            ImGui.TableNextColumn();
            ImGui.Text("Ping:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.PingMs} ms");

            ImGui.TableNextColumn();
            ImGui.Text("RTT:");
            ImGui.TableNextColumn();
            ImGui.Text($"{networkManager.RttMs} ms");
            
            ImGui.EndTable();
        }

        ImGui.End();
    }
}