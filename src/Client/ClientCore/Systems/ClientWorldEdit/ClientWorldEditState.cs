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
using Sovereign.ClientCore.Rendering.Materials;
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
    private readonly MaterialManager materialManager;

    private ulong blockTemplateId;

    public ClientWorldEditState(InputServices inputServices, MaterialManager materialManager,
        BlockTemplateEntityIndexer blockTemplateIndexer)
    {
        this.inputServices = inputServices;
        this.materialManager = materialManager;
        this.blockTemplateIndexer = blockTemplateIndexer;
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
        set => blockTemplateId = value;
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
    ///     Processes a scroll tick.
    /// </summary>
    /// <param name="isScrollUp">If true, scroll is up; if false, scroll is down.</param>
    public void OnScrollTick(bool isScrollUp)
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
}