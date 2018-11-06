/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Responsible for rendering the game scene.
    /// </summary>
    public sealed class GameSceneRenderer : IDisposable
    {

        private readonly D3D11Device device;

        private readonly GameResourceManager gameResourceManager;

        private readonly GameSceneInputAssembler inputAssembler;

        public GameSceneRenderer(D3D11Device device, GameResourceManager gameResourceManager,
            GameSceneInputAssembler inputAssembler)
        {
            this.device = device;
            this.gameResourceManager = gameResourceManager;
            this.inputAssembler = inputAssembler;
        }

        /// <summary>
        /// Initializes the game scene renderer.
        /// </summary>
        public void Initialize()
        {
            inputAssembler.Initialize();
        }

        public void Dispose()
        {
            inputAssembler.Dispose();
        }

        /// <summary>
        /// Renders the game scene.
        /// </summary>
        public void Render()
        {

        }

        /// <summary>
        /// Configures the Direct3D 11 rendering pipeline.
        /// </summary>
        private void ConfigurePipeline()
        {
            var context = device.Device.ImmediateContext;

            
        }

        private void ConfigureInputAssembler(DeviceContext context)
        {
            
        }

    }

}
