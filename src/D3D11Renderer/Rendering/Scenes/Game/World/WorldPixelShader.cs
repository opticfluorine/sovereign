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

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Sovereign.D3D11Renderer.Rendering.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Manages the main pixel shader for the game scene.
    /// </summary>
    public sealed class WorldPixelShader : IDisposable
    {

        private readonly D3D11Device device;
        private readonly GameSceneShaders shaders;
        private readonly D3D11ResourceManager mainResourceManager;

        /// <summary>
        /// Pixel shader.
        /// </summary>
        private PixelShader pixelShader;

        /// <summary>
        /// Pixel shader resource views.
        /// </summary>
        private ShaderResourceView[] resourceViews;

        /// <summary>
        /// Pixel shader sampler state.
        /// </summary>
        private SamplerState samplerState;

        public WorldPixelShader(D3D11Device device, GameSceneShaders shaders,
            D3D11ResourceManager mainResourceManager)
        {
            this.device = device;
            this.shaders = shaders;
            this.mainResourceManager = mainResourceManager;
        }

        /// <summary>
        /// Initializes the pixel shader.
        /// </summary>
        public void Initialize()
        {
            pixelShader = CreateShader();
            resourceViews = CreateResourceViews();
            samplerState = CreateSamplerState();
        }

        public void Dispose()
        {
            if (resourceViews != null)
            {
                foreach (var resourceView in resourceViews)
                {
                    resourceView.Dispose();
                }
            }
            pixelShader?.Dispose();
        }

        /// <summary>
        /// Configures the device context to use the pixel shader.
        /// </summary>
        /// <param name="context">Device context.</param>
        public void Bind(DeviceContext context)
        {
            context.PixelShader.SetShader(pixelShader, null, 0);
            context.PixelShader.SetShaderResources(0, resourceViews);
            context.PixelShader.SetSampler(0, samplerState);
        }

        public void Unbind(DeviceContext context)
        {
            context.PixelShader.SetSampler(0, null);
            for (int i = 0; i < resourceViews.Length; ++i)
            {
                context.PixelShader.SetShaderResource(i, null);
            }
            context.PixelShader.SetShader(null, null, 0);
        }

        /// <summary>
        /// Creates the D3D11 pixel shader object.
        /// </summary>
        /// <returns>Pixel shader object.</returns>
        private PixelShader CreateShader()
        {
            return new PixelShader(device.Device, shaders.WorldPixelShader);
        }

        /// <summary>
        /// Creates the shader resource views.
        /// </summary>
        /// <returns>Shader resource views.</returns>
        private ShaderResourceView[] CreateResourceViews()
        {
            var atlasTexture = mainResourceManager.AtlasTexture.Texture;

            var desc = new ShaderResourceViewDescription()
            {
                Dimension = ShaderResourceViewDimension.Texture2D,
                Format = atlasTexture.Description.Format
            };
            desc.Texture2D.MipLevels = -1;
            desc.Texture2D.MostDetailedMip = 0;

            return new ShaderResourceView[]
            {
                new ShaderResourceView(device.Device,
                    atlasTexture, desc)
            };
        }

        public SamplerState CreateSamplerState()
        {
            var desc = new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never
            };
            return new SamplerState(device.Device, desc);
        }

    }

}
