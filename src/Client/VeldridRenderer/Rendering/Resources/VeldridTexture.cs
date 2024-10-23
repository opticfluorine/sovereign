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

public enum TexturePurpose
{
    /// <summary>
    ///     Texture is intended to hold a depth buffer only.
    /// </summary>
    DepthBuffer,

    /// <summary>
    ///     Texture is intended to hold color information, used as a render target,
    ///     and used for sampling.
    /// </summary>
    RenderTexture
}

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

    /// <summary>
    ///     Creates a new texture from an existing surface.
    /// </summary>
    /// <param name="device">Device.</param>
    /// <param name="surface">Source surface.</param>
    public VeldridTexture(VeldridDevice device, Surface surface)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        Texture = CreateTexture(device, surface);

        var desc = new TextureViewDescription(Texture);
        TextureView = device.Device.ResourceFactory.CreateTextureView(desc);
    }

    /// <summary>
    ///     Creates a new empty 2D texture for the given purpose.
    /// </summary>
    /// <param name="device">Device.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="purpose">Intended purpose.</param>
    public VeldridTexture(VeldridDevice device, uint width, uint height, TexturePurpose purpose)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        Texture = CreateTexture(device, width, height, purpose);

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

    /// <summary>
    ///     Creates a texture containing pixel data from an existing surface.
    /// </summary>
    /// <param name="device">Device.</param>
    /// <param name="surface">Source surface.</param>
    /// <returns>Texture.</returns>
    private Texture CreateTexture(VeldridDevice device, Surface surface)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

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

    /// <summary>
    ///     Creates a writeable 2D texture.
    /// </summary>
    /// <param name="device">Device.</param>
    /// <param name="width">Texture width.</param>
    /// <param name="height">Texture height.</param>
    /// <param name="purpose">Intended purpose of texture.</param>
    /// <returns>Texture.</returns>
    private Texture CreateTexture(VeldridDevice device, uint width, uint height, TexturePurpose purpose)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        var format = purpose switch
        {
            TexturePurpose.RenderTexture => device.Device!.SwapchainFramebuffer.ColorTargets[0].Target.Format,
            TexturePurpose.DepthBuffer => PixelFormat.R32_Float,
            _ => PixelFormat.B8_G8_R8_A8_UNorm
        };
        var usage = purpose switch
        {
            TexturePurpose.RenderTexture => TextureUsage.Sampled | TextureUsage.RenderTarget,
            TexturePurpose.DepthBuffer => TextureUsage.DepthStencil | TextureUsage.Sampled,
            _ => TextureUsage.Storage | TextureUsage.Sampled
        };
        var desc = TextureDescription.Texture2D(width, height, 1, 1, format, usage);

        return device.Device.ResourceFactory.CreateTexture(desc);
    }
}