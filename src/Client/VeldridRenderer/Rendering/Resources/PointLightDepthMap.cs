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
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Specialized texture for a point light source.
/// </summary>
public class PointLightDepthMap : IDisposable
{
    /// <summary>
    ///     Dimension of each texture.
    /// </summary>
    private const int TextureSize = 256;

    /// <summary>
    ///     Layer index for the positive-z hemisphere.
    /// </summary>
    public const int PosZFramebufferLayer = 0;

    /// <summary>
    ///     Layer index for the negative-z hemisphere.
    /// </summary>
    public const int NegZFramebufferLayer = 1;

    /// <summary>
    ///     Number of layers in the texture.
    /// </summary>
    private const uint LayerCount = 2;

    /// <summary>
    ///     Framebuffers for rendering to the textures. Indexed by octant.
    /// </summary>
    public readonly Framebuffer[] Framebuffers;

    /// <summary>
    ///     Backing array texture.
    /// </summary>
    public readonly Texture Texture;

    /// <summary>
    ///     Texture view.
    /// </summary>
    public readonly TextureView TextureView;

    public PointLightDepthMap(VeldridDevice device)
    {
        var textureDesc = TextureDescription.Texture2D(TextureSize, TextureSize, 1, LayerCount, PixelFormat.R32_Float,
            TextureUsage.DepthStencil | TextureUsage.Sampled);
        Texture = device.Device!.ResourceFactory.CreateTexture(textureDesc);

        Framebuffers = new Framebuffer[LayerCount];
        for (var i = 0; i < LayerCount; ++i)
        {
            var depthAttachmentDesc = new FramebufferAttachmentDescription(Texture, (uint)i);
            var fbDesc = new FramebufferDescription
            {
                DepthTarget = depthAttachmentDesc
            };
            Framebuffers[i] = device.Device!.ResourceFactory.CreateFramebuffer(fbDesc);
        }

        var texViewDesc = new TextureViewDescription(Texture);
        TextureView = device.Device!.ResourceFactory.CreateTextureView(texViewDesc);
    }

    public void Dispose()
    {
        for (var i = 0; i < LayerCount; ++i) Framebuffers[i].Dispose();
        TextureView.Dispose();
        Texture.Dispose();
    }
}