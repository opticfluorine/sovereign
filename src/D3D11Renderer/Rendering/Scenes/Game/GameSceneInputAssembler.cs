﻿/*
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
    /// Manages the input assembler stage for the game scene.
    /// </summary>
    public sealed class GameSceneInputAssembler : IDisposable
    {

        private readonly D3D11Device device;

        private readonly GameSceneShaders shaders;

        /// <summary>
        /// Vertex buffer input layout.
        /// </summary>
        private InputLayout inputLayout;

        public GameSceneInputAssembler(D3D11Device device, GameSceneShaders shaders)
        {
            this.device = device;
            this.shaders = shaders;
        }

        /// <summary>
        /// Initializes the input assembler manager.
        /// </summary>
        public void Initialize()
        {
            inputLayout = CreateInputLayout();
        }

        public void Dispose()
        {
            inputLayout?.Dispose();
        }

        /// <summary>
        /// Configures the input assembler stage.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Configure(DeviceContext context)
        {
            context.InputAssembler.InputLayout = inputLayout;
        }

        /// <summary>
        /// Creates the input layout for the vertex buffer.
        /// </summary>
        private InputLayout CreateInputLayout()
        {
            var layouts = new InputElement[]
            {
                /* Vertex buffer */
                new InputElement
                {
                    AlignedByteOffset = InputElement.AppendAligned,
                    Classification = InputClassification.PerVertexData,
                    Format = SharpDX.DXGI.Format.R32G32B32_Float,
                    InstanceDataStepRate = 0,
                    Slot = 0,
                    SemanticName = "POSITION",
                    SemanticIndex = 0
                },

                /* Texture coordinate buffer */
                new InputElement
                {
                    AlignedByteOffset = InputElement.AppendAligned,
                    Classification = InputClassification.PerVertexData,
                    Format = SharpDX.DXGI.Format.R32G32_Float,
                    InstanceDataStepRate = 0,
                    Slot = 1,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0
                },
            };
            return new InputLayout(device.Device, shaders.WorldVertexShader, layouts);
        }

    }

}