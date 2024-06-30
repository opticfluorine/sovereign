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
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Systems.Input;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Tracks the current state for the client-side world editor.
/// </summary>
public class ClientWorldEditState
{
    /// <summary>
    ///     Minimum value of Z offset.
    /// </summary>
    private const int MinZOffset = -10;

    /// <summary>
    ///     Maximum value of Z offset.
    /// </summary>
    private const int MaxZOffset = 10;

    private readonly InputServices inputServices;
    private readonly MaterialManager materialManager;

    public ClientWorldEditState(InputServices inputServices, MaterialManager materialManager)
    {
        this.inputServices = inputServices;
        this.materialManager = materialManager;
    }

    /// <summary>
    ///     Selected material.
    /// </summary>
    public int Material { get; private set; }

    /// <summary>
    ///     Selected material modifier.
    /// </summary>
    public int MaterialModifier { get; private set; }

    /// <summary>
    ///     Selected Z offset for editing relative to camera.
    /// </summary>
    public int ZOffset { get; private set; }

    /// <summary>
    ///     Processes a scroll tick.
    /// </summary>
    /// <param name="isScrollUp">If true, scroll is up; if false, scroll is down.</param>
    public void OnScrollTick(bool isScrollUp)
    {
        // Scroll while holding CTRL varies the z-offset.
        // Scrolling without holding CTRL varies the material and material modifier.
        var ctrlPressed = inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_LCTRL)
                          || inputServices.IsKeyDown(SDL.SDL_Keycode.SDLK_RCTRL);
        if (ctrlPressed)
        {
            // Z offset scroll.
            ZOffset = Math.Min(Math.Max(ZOffset + (isScrollUp ? 1 : -1), MinZOffset), MaxZOffset);
        }
        else
        {
            // Material/modifier scroll.
            var newModifier = MaterialModifier + (isScrollUp ? 1 : -1);
            if (newModifier >= materialManager.Materials[Material].MaterialSubtypes.Count)
            {
                if (Material < materialManager.Materials.Count - 1)
                {
                    Material++;
                    MaterialModifier = 0;
                }
            }
            else if (newModifier < 0)
            {
                if (Material > 1)
                {
                    Material--;
                    MaterialModifier = materialManager.Materials[Material].MaterialSubtypes.Count - 1;
                }
            }
            else
            {
                MaterialModifier = newModifier;
            }
        }
    }
}