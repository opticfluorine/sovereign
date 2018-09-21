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

using Sovereign.ClientCore.Rendering;
using Sovereign.D3D11Renderer.Rendering.Configuration;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Resources.Textures
{

    /// <summary>
    /// Manages an immutable Direct3D 11 texture.
    /// </summary>
    public class D3D11Texture : IDisposable
    {

        /// <summary>
        /// Texture.
        /// </summary>
        public Texture2D Texture { get; private set; }

        /// <summary>
        /// Creates a texture from the given surface.
        /// </summary>
        /// <param name="device">Device for which the texture is to be created.</param>
        /// <param name="surface">Surface to create texture from.</param>
        public D3D11Texture(D3D11Device device, Surface surface)
        {
            /* Attempt to create the texture. */
            Texture = CreateTexture(device, surface);
        }

        public void Dispose()
        {
            Texture.Dispose();
        }

        private Texture2D CreateTexture(D3D11Device device, Surface surface)
        {
            /* Define the texture description. */
            var desc = new Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = D3D11RendererConstants.DisplayFormat,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Immutable,
                Width = surface.Properties.Width,
                Height = surface.Properties.Height,
            };

            /* Prepare the data. */
            var data = new SharpDX.DataRectangle()
            {
                DataPointer = surface.Properties.Data,
                Pitch = surface.Properties.Pitch,
            };

            /* Create the texture. */
            return new Texture2D(device.Device, desc, data);
        }

    }

}
