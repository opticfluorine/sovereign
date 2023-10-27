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
using System.Collections.Generic;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Wrapper class for Veldrid 2D textures intended for color sampling.
/// </summary>
public class VeldridTexture : IDisposable
{
    /// <summary>
    ///     Map from internal pixel format to Veldrid pixel format.
    /// </summary>
    private static readonly IDictionary<DisplayFormat, PixelFormat> formatMap
        = new Dictionary<DisplayFormat, PixelFormat>
        {
            { DisplayFormat.R8G8B8A8_UNorm, PixelFormat.R8_G8_B8_A8_UNorm },
            { DisplayFormat.B8G8R8A8_UNorm, PixelFormat.B8_G8_R8_A8_UNorm }
        };

    public VeldridTexture(VeldridDevice device, Surface surface)
    {
        Texture = CreateTexture(device, surface);

        var desc = new TextureViewDescription(Texture);
        TextureView = device.Device.ResourceFactory.CreateTextureView(desc);
    }

    /// <summary>
    ///     Backing Veldrid texture.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    ///     TextureView for the texture.
    /// </summary>
    public TextureView TextureView { get; }

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