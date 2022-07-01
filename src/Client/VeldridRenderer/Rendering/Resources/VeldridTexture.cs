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
using System.Collections.Generic;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
/// Wrapper class for Veldrid 2D textures intended for color sampling.
/// </summary>
public class VeldridTexture : IDisposable
{

    /// <summary>
    /// Backing Veldrid texture.
    /// </summary>
    public Texture Texture { get; private set; }

    /// <summary>
    /// TextureView for the texture.
    /// </summary>
    public TextureView TextureView { get; private set; }

    /// <summary>
    /// Map from internal pixel format to Veldrid pixel format.
    /// </summary>
    private static IDictionary<DisplayFormat, PixelFormat> formatMap
        = new Dictionary<DisplayFormat, PixelFormat>()
        {
            {DisplayFormat.R8G8B8A8_UNorm, PixelFormat.R8_G8_B8_A8_UNorm},
            {DisplayFormat.B8G8R8A8_UNorm, PixelFormat.B8_G8_R8_A8_UNorm}
        };

    public VeldridTexture(VeldridDevice device, Surface surface)
    {
        Texture = CreateTexture(device, surface);

        var desc = new TextureViewDescription(Texture);
        TextureView = device.Device.ResourceFactory.CreateTextureView(desc);
    }

    public void Dispose()
    {
        TextureView.Dispose();
        Texture.Dispose();
    }

    private Texture CreateTexture(VeldridDevice device, Surface surface)
    {
        /* Allocate a texture on the GPU. */
        var desc = TextureDescription.Texture2D((uint)surface.Properties.Width,
            (uint)surface.Properties.Height, 1, 1, formatMap[surface.Properties.Format],
            TextureUsage.Sampled);

        var texture = device.Device.ResourceFactory.CreateTexture(desc);

        /* Copy pixel data into the texture. */
        device.Device.UpdateTexture(texture,
            surface.Properties.Data,
            surface.Properties.BytesPerPixel * (uint)surface.Properties.Width * (uint)surface.Properties.Height,
            0, 0, 0,
            (uint)surface.Properties.Width, (uint)surface.Properties.Height, 1,
            0, 0);

        return texture;
    }

}
