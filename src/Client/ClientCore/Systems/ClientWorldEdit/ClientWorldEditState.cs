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
using SDL2;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Tracks the current state for the client-side world editor.
/// </summary>
public class ClientWorldEditState
{
    private readonly BlockTemplateEntityIndexer blockTemplateIndexer;
    private readonly InputServices inputServices;
    private readonly NpcTemplateEntityIndexer npcTemplateIndexer;

    private ulong blockTemplateId;
    private ulong npcTemplateId;

    public ClientWorldEditState(InputServices inputServices, BlockTemplateEntityIndexer blockTemplateIndexer,
        NpcTemplateEntityIndexer npcTemplateIndexer)
    {
        this.inputServices = inputServices;
        this.blockTemplateIndexer = blockTemplateIndexer;
        this.npcTemplateIndexer = npcTemplateIndexer;
    }

    /// <summary>
    ///     Selected block template.
    /// </summary>
    public ulong BlockTemplateId
    {
        get
        {
            // Lazy load of first block template entity ID.
            if (blockTemplateId < EntityConstants.FirstTemplateEntityId)
                blockTemplateId = blockTemplateIndexer.First;

            return blockTemplateId;
        }
        private set => blockTemplateId = value;
    }

    /// <summary>
    ///     Selected Z offset for editing relative to camera.
    /// </summary>
    public int ZOffset { get; private set; }

    /// <summary>
    ///     Pen width in blocks.
    /// </summary>
    public int PenWidth { get; private set; } = ClientWorldEditConstants.MinPenWidth;

    /// <summary>
    ///     Currently selected tool for editing.
    /// </summary>
    public WorldEditTool WorldEditTool { get; private set; } = WorldEditTool.Block;

    /// <summary>
    ///     Selected NPC template.
    /// </summary>
    public ulong NpcTemplateId
    {
        get
        {
            // Lazy load of first block template entity ID.
            if (npcTemplateId < EntityConstants.FirstTemplateEntityId)
                npcTemplateId = npcTemplateIndexer.First;

            return npcTemplateId;
        }

        private set => npcTemplateId = value;
    }

    /// <summary>
    ///     Whether world editing is snapped to grid.
    /// </summary>
    public bool SnapToGrid { get; private set; }

    /// <summary>
    ///     Sets the snap-to-grid value.
    /// </summary>
    /// <param name="snapToGrid">Snap to grid value.</param>
    public void SetSnapToGrid(bool snapToGrid)
    {
        SnapToGrid = snapToGrid;
    }

    /// <summary>
    ///     Processes a scroll tick.
    /// </summary>
    /// <param name="isScrollUp">If true, scroll is up; if false, scroll is down.</param>
    public void OnScrollTick(bool isScrollUp)
    {
        switch (WorldEditTool)
        {
            case WorldEditTool.Block:
                HandleScrollForBlockTool(isScrollUp);
                break;
            case WorldEditTool.Npc:
                HandleScrollTickForNpcTool(isScrollUp);
                break;
            case WorldEditTool.Item:
                HandleScrollTickForItemTool(isScrollUp);
                break;
        }
    }

    /// <summary>
    ///     Sets the Z offset. Does not perform any validation.
    /// </summary>
    /// <param name="zOffset">Z-offset.</param>
    public void SetZOffset(int zOffset)
    {
        ZOffset = zOffset;
    }

    /// <summary>
    ///     Sets the pen width. Does not perform any validation.
    /// </summary>
    /// <param name="penWidth">Pen width.</param>
    public void SetPenWidth(int penWidth)
    {
        PenWidth = penWidth;
    }

    /// <summary>
    ///     Sets the current tool. Does not perform any validation.
    /// </summary>
    /// <param name="worldEditTool">Tool.</param>
    public void SetTool(WorldEditTool worldEditTool)
    {
        WorldEditTool = worldEditTool;
    }

    /// <summary>
    ///     Handles a scroll tick for the block tool.
    /// </summary>
    /// <param name="isScrollUp">If true, mouse wheel scrolled up; down otherwise.</param>
    private void HandleScrollForBlockTool(bool isScrollUp)
    {
        // The following rules are applied in priority order:
        // Scroll while holding CTRL varies the z-offset.
        // Scroll while holding SHIFT varies the pen width.
        // Scrolling without holding keys varies the material and material modifier.
        var ctrlPressed = inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_LCTRL)
                          || inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_RCTRL);
        var shiftPressed = inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_LSHIFT)
                           || inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_RSHIFT);
        if (ctrlPressed)
        {
            // Z offset scroll.
            ZOffset = Math.Min(Math.Max(ZOffset + (isScrollUp ? 1 : -1), ClientWorldEditConstants.MinZOffset),
                ClientWorldEditConstants.MaxZOffset);
        }
        else if (shiftPressed)
        {
            // Pen width scroll.
            PenWidth = Math.Min(Math.Max(PenWidth + (isScrollUp ? 1 : -1), ClientWorldEditConstants.MinPenWidth),
                ClientWorldEditConstants.MaxPenWidth);
        }
        else
        {
            // Template entity ID scroll.
            if (!(isScrollUp
                    ? blockTemplateIndexer.TryGetNextLarger(BlockTemplateId, out var next)
                    : blockTemplateIndexer.TryGetNextSmaller(BlockTemplateId, out next))) return;

            BlockTemplateId = next;
        }
    }

    /// <summary>
    ///     Handles a scroll tick for the NPC tool.
    /// </summary>
    /// <param name="isScrollUp">If true, mouse wheel scrolled up; down otherwise.</param>
    private void HandleScrollTickForNpcTool(bool isScrollUp)
    {
        if (!(isScrollUp
                ? npcTemplateIndexer.TryGetNextLarger(NpcTemplateId, out var next)
                : npcTemplateIndexer.TryGetNextSmaller(NpcTemplateId, out next))) return;

        NpcTemplateId = next;
    }

    /// <summary>
    ///     Handles a scroll tick for the item tool.
    /// </summary>
    /// <param name="isScrollUp">If true, mouse wheel scrolled up; down otherwise.</param>
    private void HandleScrollTickForItemTool(bool isScrollUp)
    {
    }
}