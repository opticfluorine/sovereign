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
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Resources;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Scene consumer used to render the in-game scene via Veldrid.
/// </summary>
public class GameSceneConsumer : ISceneConsumer, IDisposable
{
    private readonly GameResourceManager gameResManager;
    private readonly GameSceneRenderer renderer;
    private readonly VeldridResourceManager resManager;
    private readonly WorldVertexConstantsUpdater worldVcUpdater;

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public GameSceneConsumer(GameResourceManager gameResManager,
        VeldridResourceManager resManager, GameSceneRenderer renderer,
        WorldVertexConstantsUpdater worldVcUpdater)
    {
        this.resManager = resManager;
        this.gameResManager = gameResManager;
        this.renderer = renderer;
        this.worldVcUpdater = worldVcUpdater;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            renderer.Dispose();
            gameResManager.Dispose();
            isDisposed = true;
        }
    }

    public void ConsumeScene(IScene scene)
    {
        if (gameResManager.VertexBuffer == null || gameResManager.IndexBuffer == null ||
            gameResManager.RenderPlan == null)
            throw new InvalidOperationException("Buffers not ready.");
        if (resManager.CommandList == null)
            throw new InvalidOperationException("Command list not ready.");

        /* General processing. */
        var renderPlan = gameResManager.RenderPlan;
        renderPlan.Reset();
        scene.BuildRenderPlan(renderPlan);

        gameResManager.VertexBuffer.UsedLength = (uint)renderPlan.VertexCount;
        gameResManager.IndexBuffer.UsedLength = (uint)renderPlan.IndexCount;
        worldVcUpdater.Update(scene);

        /* Post updates to buffers. */
        var commandList = resManager.CommandList;
        gameResManager.UpdateBuffers(commandList);

        /* Render. */
        renderer.Render(commandList, renderPlan);
    }

    /// <summary>
    ///     Initializes the game scene.
    /// </summary>
    public void Initialize()
    {
        gameResManager.Initialize();
        renderer.Initialize();
    }
}