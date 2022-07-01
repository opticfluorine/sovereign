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
using Sovereign.VeldridRenderer.Rendering.Scenes.Game;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
/// Primary scene consumer for the Veldrid renderer.
/// </summary>
public class VeldridSceneConsumer : ISceneConsumer, IDisposable
{
    private readonly GameSceneConsumer gameSceneConsumer;

    /// <summary>
    /// Dispose flag.
    /// </summary>
    private bool isDisposed = false;

    public VeldridSceneConsumer(GameSceneConsumer gameSceneConsumer)
    {
        this.gameSceneConsumer = gameSceneConsumer;
    }

    /// <summary>
    /// Initializes the scene tree.
    /// </summary>
    public void Initialize()
    {
        this.gameSceneConsumer.Initialize();
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            this.gameSceneConsumer.Dispose();
            isDisposed = true;
        }
    }

    public void ConsumeScene(IScene scene)
    {
        /* Dispatch. */
        switch (scene.SceneType)
        {
            case SceneType.Game:
                gameSceneConsumer.ConsumeScene(scene);
                break;

            default:
                /* Unsupported scene. */
                break;
        }
    }
}
