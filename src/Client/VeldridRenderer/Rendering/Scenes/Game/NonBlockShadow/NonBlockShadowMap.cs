// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using Sovereign.VeldridRenderer.Rendering.Resources;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game.NonBlockShadow;

/// <summary>
///     Holds resources for the non-block-entity shadow map.
/// </summary>
public class NonBlockShadowMap : IDisposable
{
    /// <summary>
    ///     Non-block-entity shadow map texture width.
    /// </summary>
    private const int Width = 1024;

    /// <summary>
    ///     Non-block-entity shadow map texture height.
    /// </summary>
    private const int Height = 1024;

    public NonBlockShadowMap(VeldridDevice device)
    {
        NonBlockShadowMapTexture = new Lazy<VeldridTexture>(() =>
            new VeldridTexture(device, Width, Height, TexturePurpose.DepthBuffer));

        Framebuffer = new Lazy<Framebuffer>(() =>
        {
            var desc = new FramebufferDescription(NonBlockShadowMapTexture.Value.Texture);
            return device.Device!.ResourceFactory.CreateFramebuffer(desc);
        });

        ConstantsBuffer = new Lazy<VeldridUpdateBuffer<NonBlockShadowShaderConstants>>(() =>
            new VeldridUpdateBuffer<NonBlockShadowShaderConstants>(device,
                BufferUsage.UniformBuffer, 1));
    }

    /// <summary>
    ///     Veldrid texture used for holding the non-block-entity shadow map for each frame.
    /// </summary>
    public Lazy<VeldridTexture> NonBlockShadowMapTexture { get; }

    /// <summary>
    ///     Shader constants buffer.
    /// </summary>
    public Lazy<VeldridUpdateBuffer<NonBlockShadowShaderConstants>> ConstantsBuffer { get; }

    /// <summary>
    ///     Framebuffer for rendering into the shadow map.
    /// </summary>
    public Lazy<Framebuffer> Framebuffer { get; }

    public void Dispose()
    {
        if (NonBlockShadowMapTexture.IsValueCreated) NonBlockShadowMapTexture.Value.Dispose();
        if (ConstantsBuffer.IsValueCreated) ConstantsBuffer.Value.Dispose();
    }
}