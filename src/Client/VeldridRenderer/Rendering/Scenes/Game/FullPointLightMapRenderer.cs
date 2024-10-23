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
using Sovereign.ClientCore.Rendering;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Renderer for the per-layer full point light map.
/// </summary>
public class FullPointLightMapRenderer : IDisposable
{
    private readonly VeldridDevice device;
    private readonly GameResourceManager gameResMgr;
    private readonly Lazy<Framebuffer> mapFramebuffer;
    private readonly uint[] offsets = new uint[1];
    private readonly Lazy<ResourceSet> resourceSet;

    public FullPointLightMapRenderer(VeldridDevice device, GameResourceManager gameResMgr)
    {
        this.device = device;
        this.gameResMgr = gameResMgr;

        mapFramebuffer = new Lazy<Framebuffer>(() =>
        {
            var desc = new FramebufferDescription(null, gameResMgr.FullPointLightMap.Value.Texture);
            return device.Device!.ResourceFactory.CreateFramebuffer(desc);
        });

        Lazy<ResourceLayout> resourceLayout = new(() =>
        {
            var desc = new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(nameof(PointLightShaderConstants),
                    ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex | ShaderStages.Fragment,
                    ResourceLayoutElementOptions.DynamicBinding),
                new ResourceLayoutElementDescription(nameof(PointLightDepthMap), ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment)
            );
            return device.Device!.ResourceFactory.CreateResourceLayout(desc);
        });

        resourceSet = new Lazy<ResourceSet>(() =>
        {
            var desc = new ResourceSetDescription(resourceLayout.Value,
                gameResMgr.PointLightBuffer!.DeviceBuffer);
            return device.Device!.ResourceFactory.CreateResourceSet(desc);
        });
    }

    public void Dispose()
    {
    }

    /// <summary>
    ///     Renders the full point light map for the current layer.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void Render(CommandList commandList, RenderPlan renderPlan)
    {
        commandList.SetFramebuffer(mapFramebuffer.Value);
        commandList.ClearColorTarget(0, RgbaFloat.Clear);

        for (var i = 0; i < renderPlan.LightCount; ++i)
        {
            offsets[0] = gameResMgr.PointLightBuffer!.GetOffset(i);
            commandList.SetGraphicsResourceSet(0, resourceSet.Value, offsets);
        }
    }
}