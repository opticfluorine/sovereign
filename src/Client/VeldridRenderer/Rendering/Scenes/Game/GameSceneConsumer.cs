/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.VeldridRenderer.Rendering.Resources;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
/// Scene consumer used to render the in-game scene via Veldrid.
/// </summary>
public class GameSceneConsumer : ISceneConsumer, IDisposable
{
    private readonly VeldridResourceManager resManager;
    private readonly GameResourceManager gameResManager;
    private readonly GameSceneRenderer renderer;
    private readonly WorldVertexConstantsUpdater worldVcUpdater;

    /// <summary>
    /// Dispose flag.
    /// </summary>
    private bool isDisposed = false;

    public GameSceneConsumer(GameResourceManager gameResManager,
        VeldridResourceManager resManager, GameSceneRenderer renderer,
        WorldVertexConstantsUpdater worldVcUpdater)
    {
        this.resManager = resManager;
        this.gameResManager = gameResManager;
        this.renderer = renderer;
        this.worldVcUpdater = worldVcUpdater;
    }

    /// <summary>
    /// Initializes the game scene.
    /// </summary>
    public void Initialize()
    {
        this.gameResManager.Initialize();
        this.renderer.Initialize();
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            this.renderer.Dispose();
            this.gameResManager.Dispose();
            isDisposed = true;
        }
    }

    public void ConsumeScene(IScene scene)
    {
        /* General processing. */
        scene.PopulateBuffers(gameResManager.VertexBuffer.Buffer,
            gameResManager.IndexBuffer.Buffer,
            gameResManager.DrawBuffer,
            out var drawCount);
        gameResManager.DrawCount = drawCount;
        worldVcUpdater.Update(scene);

        /* Post updates to buffers. */
        var commandList = resManager.CommandList;
        gameResManager.UpdateBuffers(commandList);

        /* Render. */
        renderer.Render(commandList);
    }
}
