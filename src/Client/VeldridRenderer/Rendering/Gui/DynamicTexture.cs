// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     A dynamically updated texture managed by <see cref="VeldridDynamicTextureManager"/>.
/// </summary>
internal sealed class DynamicTexture : IDisposable
{
    public readonly uint Height;

    public readonly DynamicTextureSampling Sampling;

    public readonly uint Width;

    private readonly VeldridDevice device;

    public DynamicTexture(VeldridDevice device, uint width, uint height, DynamicTextureSampling sampling)
    {
        this.device = device;
        Width = width;
        Height = height;
        Sampling = sampling;
    }

    /// <summary>
    ///     Backing Veldrid texture. Null until the add operation has been applied
    ///     on the render thread.
    /// </summary>
    public VeldridTexture? Texture { get; private set; }

    /// <summary>
    ///     Resource set binding this texture into the GUI pipeline. Null until the
    ///     add operation has been applied on the render thread.
    /// </summary>
    public ResourceSet? ResourceSet { get; set; }

    public void Dispose()
    {
        ResourceSet?.Dispose();
        Texture?.Dispose();
    }

    /// <summary>
    ///     Creates the GPU texture. Pixel data is uploaded separately by the manager
    ///     via UpdateTexture operations.
    /// </summary>
    public void CreateGpuTexture()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        var desc = TextureDescription.Texture2D(Width, Height, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
        var texture = device.Device.ResourceFactory.CreateTexture(desc);

        Texture = new VeldridTexture(device, texture);
    }

    /// <summary>
    ///     Uploads pixel data to the GPU texture. Render thread only, after
    ///     <see cref="CreateGpuTexture"/>.
    /// </summary>
    /// <param name="pixels">RGBA8 pixel data, exactly Width * Height * 4 bytes.</param>
    public unsafe void UploadPixels(ReadOnlySpan<byte> pixels)
    {
        if (Texture == null)
            throw new InvalidOperationException("GPU texture not created yet.");
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");

        fixed (byte* ptr = pixels)
        {
            device.Device.UpdateTexture(Texture.Texture, (IntPtr)ptr,
                (uint)pixels.Length, 0, 0, 0, Width, Height, 1, 0, 0);
        }
    }
}
