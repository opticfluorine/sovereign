/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Sovereign.ClientCore.Rendering;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

public class GameSceneRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly WorldRenderer worldRenderer;

    public GameSceneRenderer(VeldridDevice device, WorldRenderer worldRenderer)
    {
        this.device = device;
        this.worldRenderer = worldRenderer;
    }

    public void Dispose()
    {
        worldRenderer.Dispose();
    }

    /// <summary>
    ///     Initializes the game scene renderer.
    /// </summary>
    public void Initialize()
    {
        worldRenderer.Initialize();
    }

    /// <summary>
    ///     Renders the game scene.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan)
    {
        worldRenderer.Render(commandList, renderPlan);
    }
}