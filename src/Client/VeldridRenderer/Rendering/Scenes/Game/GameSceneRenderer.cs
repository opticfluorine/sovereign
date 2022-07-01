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

    /// <summary>
    /// Initializes the game scene renderer.
    /// </summary>
    public void Initialize()
    {
        worldRenderer.Initialize();
    }

    public void Dispose()
    {
        worldRenderer.Dispose();
    }

    /// <summary>
    /// Renders the game scene.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Render(CommandList commandList)
    {
        worldRenderer.Render(commandList);
    }

}
