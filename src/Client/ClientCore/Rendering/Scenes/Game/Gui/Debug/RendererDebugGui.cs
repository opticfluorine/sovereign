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

using System;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;

/// <summary>
///     Debug GUI for renderer debugging.
/// </summary>
public sealed class RendererDebugGui(DebugState debugState, IBlockAnimatedSpriteCache tileCache)
{
    private const int GridPosSize = 3;
    private readonly int[] tileCoords = new int[GridPosSize];
    private bool tileSpriteLogContext = true;
    private bool tileSpriteSelf = true;

    /// <summary>
    ///     Renders the renderer debug GUI.
    /// </summary>
    public void Render()
    {
        if (!ImGui.Begin("Renderer Debug", ImGuiWindowFlags.AlwaysAutoResize)) return;

        DepthControl();
        SpriteControl();
        TileSpriteControl();

        ImGui.End();
    }

    /// <summary>
    ///     Renders sprite control debug options.
    /// </summary>
    private void SpriteControl()
    {
        ImGui.SeparatorText("Sprite Control");
        var inhibitFront = debugState.InhibitBlockFrontFaces;
        var inhibitTop = debugState.InhibitBlockTopFaces;
        var inhibitNonBlock = debugState.InhibitNonBlocks;

        ImGui.Checkbox("Inhibit Block Front Faces", ref inhibitFront);
        ImGui.Checkbox("Inhibit Block Top Faces", ref inhibitTop);
        ImGui.Checkbox("Inhibit Non-Block Sprites", ref inhibitNonBlock);

        debugState.InhibitBlockFrontFaces = inhibitFront;
        debugState.InhibitBlockTopFaces = inhibitTop;
        debugState.InhibitNonBlocks = inhibitNonBlock;
    }

    /// <summary>
    ///     Renders depth control debug options.
    /// </summary>
    private void DepthControl()
    {
        ImGui.SeparatorText("Depth Control");
        var enableZCap = debugState.EnableZLayerCap;
        ImGui.Checkbox("Enable Z Layer Cap", ref enableZCap);
        debugState.EnableZLayerCap = enableZCap;

        var zCapOffset = debugState.ZLayerCapOffset;
        ImGui.Text("Z Layer Cap Offset:");
        ImGui.SameLine();
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 4.0f);
        ImGui.InputInt("##zOff", ref zCapOffset);
        debugState.ZLayerCapOffset = zCapOffset;
    }

    /// <summary>
    ///     Renders tile sprite control debug options.
    /// </summary>
    private void TileSpriteControl()
    {
        ImGui.SeparatorText("Tile Sprite Control");

        InputTileCoords();
        ImGui.Checkbox("Include Self", ref tileSpriteSelf);
        ImGui.SameLine();
        ImGui.Checkbox("Log Tile Contexts", ref tileSpriteLogContext);

        if (ImGui.Button("Refresh Tile Cache for Block"))
        {
            debugState.LogTileSpriteContexts = tileSpriteLogContext;
            tileCache.UpdateCacheForBlock(new GridPosition(tileCoords), tileSpriteSelf);
            debugState.LogTileSpriteContexts = false;
        }
    }

    /// <summary>
    ///     Renders control for input of the tile sprite refresh block position.
    /// </summary>
    private unsafe void InputTileCoords()
    {
        var buf = stackalloc int[GridPosSize];
        Marshal.Copy(tileCoords, 0, (IntPtr)buf, GridPosSize);
        ImGui.InputInt3("Block Position", buf);
        Marshal.Copy((IntPtr)buf, tileCoords, 0, GridPosSize);
    }
}