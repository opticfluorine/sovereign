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
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Timing;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Sovereign.VeldridRenderer.Rendering.Scenes.Game;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Handles rendering of the GUI layer.
/// </summary>
public class GuiRenderer : IDisposable
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly TextureAtlasManager atlasManager;
    private readonly AtlasMap atlasMap;
    private readonly ClientConfigurationManager configManager;
    private readonly VeldridDevice device;
    private readonly CommonGuiManager guiManager;
    private readonly GuiPipeline guiPipeline;
    private readonly GuiResourceManager guiResourceManager;
    private readonly VeldridResourceManager resourceManager;
    private readonly ISystemTimer systemTimer;
    private readonly GuiTextureMapper textureMapper;

    /// <summary>
    ///     Last bound texture.
    /// </summary>
    private IntPtr lastTexture = IntPtr.Zero;

    /// <summary>
    ///     Resource set for GUI rendering.
    /// </summary>
    private ResourceSet? resourceSet;

    private Vector2 scaleFactor;

    /// <summary>
    ///     Working copy of the vertex shader constants.
    /// </summary>
    private GuiVertexShaderConstants vertexConstants;

    public GuiRenderer(CommonGuiManager guiManager, GuiResourceManager guiResourceManager, GuiPipeline guiPipeline,
        TextureAtlasManager atlasManager, ISystemTimer systemTimer, AnimatedSpriteManager animatedSpriteManager,
        AtlasMap atlasMap, VeldridDevice device, VeldridResourceManager resourceManager,
        ClientConfigurationManager configManager, GuiTextureMapper textureMapper)
    {
        this.guiManager = guiManager;
        this.guiResourceManager = guiResourceManager;
        this.guiPipeline = guiPipeline;
        this.atlasManager = atlasManager;
        this.systemTimer = systemTimer;
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.device = device;
        this.resourceManager = resourceManager;
        this.configManager = configManager;
        this.textureMapper = textureMapper;
    }

    public void Dispose()
    {
        resourceSet?.Dispose();
        guiPipeline.Dispose();
        guiResourceManager.Dispose();
    }

    /// <summary>
    ///     Initializes the GUI renderer and its resources.
    /// </summary>
    public void Initialize()
    {
        if (device.DisplayMode == null)
            throw new InvalidOperationException("Display mode is null.");

        // Initialize resources.
        guiResourceManager.Initialize();
        guiPipeline.Initialize();
        CreateResourceSet();

        // Compute constant projection matrix.
        var io = ImGui.GetIO();
        vertexConstants.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            0.0f,
            io.DisplaySize.Y,
            -1.0f,
            1.0f
        );
        scaleFactor = new Vector2(configManager.ClientConfiguration.Display.UiScaleFactor);
        io.DisplaySize = new Vector2(device.DisplayMode.Width / scaleFactor.X,
            device.DisplayMode.Height / scaleFactor.Y);
        io.DisplayFramebufferScale = scaleFactor;
    }

    /// <summary>
    ///     Ends the current frame after all rendering is complete.
    /// </summary>
    public void EndFrame()
    {
        guiResourceManager.EndFrame();
    }

    /// <summary>
    ///     Renders the GUI layer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void RenderGui(CommandList commandList)
    {
        if (guiResourceManager.GuiUniformBuffer == null)
            throw new InvalidOperationException("GUI uniform buffer is null.");
        if (resourceSet == null)
            throw new InvalidOperationException("GUI resource set is null.");

        // First stage, convert pending ImGui commands to drawing-level data
        var drawData = guiManager.Render();
        if (drawData.CmdListsCount == 0) return;
        drawData.ScaleClipRects(scaleFactor);

        // Update and bind resources used across all GUI draw calls.
        commandList.PushDebugGroup("GUI");
        UpdateBuffers(commandList, drawData);
        commandList.SetPipeline(guiPipeline.Pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);

        // Execute draw commands.
        var vertexOffset = 0;
        var indexOffset = 0;
        var systemTime = systemTimer.GetTime();
        var clipOffset = drawData.DisplayPos;
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];
            var listIndexOffset = 0;
            for (var j = 0; j < curList.CmdBuffer.Size; ++j)
            {
                var curCmd = curList.CmdBuffer[j];

                // Check scissor rect for a positive area.
                // If the area isn't positive, then there is nothing to draw with this call.
                var minX = Math.Max(curCmd.ClipRect.X - clipOffset.X, 0f);
                var minY = Math.Max(curCmd.ClipRect.Y - clipOffset.Y, 0f);
                var maxX = curCmd.ClipRect.Z - clipOffset.X;
                var maxY = curCmd.ClipRect.W - clipOffset.Y;
                if (maxX <= minX || maxY <= minY) continue;

                commandList.SetScissorRect(0,
                    (uint)minX,
                    (uint)minY,
                    (uint)(maxX - minX),
                    (uint)(maxY - minY));

                // Resource binding for next draw call.
                if (curCmd.TextureId != GuiFontAtlas.TextureId)
                {
                    var texId = curCmd.GetTexID();
                    var texData = textureMapper.GetTextureDataForTextureId(texId);
                    if (texData is { SourceType: GuiTextureMapper.SourceType.Multiple, Layers: not null })
                    {
                        foreach (var layerTexId in texData.Layers)
                        {
                            DrawTextureLayer(commandList, curCmd, layerTexId, systemTime, indexOffset, listIndexOffset,
                                vertexOffset);
                        }
                    }
                    else
                    {
                        DrawTextureLayer(commandList, curCmd, texId, systemTime, indexOffset, listIndexOffset,
                            vertexOffset);
                    }
                }
                else
                {
                    DrawTextureLayer(commandList, curCmd, curCmd.GetTexID(), systemTime, indexOffset, listIndexOffset,
                        vertexOffset);
                }

                listIndexOffset += (int)curCmd.ElemCount;
            }

            vertexOffset += curList.VtxBuffer.Size;
            indexOffset += curList.IdxBuffer.Size;
        }

        commandList.PopDebugGroup();
    }

    /// <summary>
    ///     Draws a GUI texture layer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="curCmd">Current ImGui draw command.</param>
    /// <param name="texId">Texture ID for current layer.</param>
    /// <param name="systemTime">Current frame system time.</param>
    /// <param name="indexOffset">Overall offset into GUI index buffer.</param>
    /// <param name="listIndexOffset">Current list relative offset into GUI index buffer.</param>
    /// <param name="vertexOffset">Overall offset into GUI vertex buffer.</param>
    private void DrawTextureLayer(CommandList commandList, ImDrawCmdPtr curCmd, IntPtr texId, ulong systemTime,
        int indexOffset,
        int listIndexOffset, int vertexOffset)
    {
        if (guiResourceManager.GuiUniformBuffer == null)
            throw new InvalidOperationException("GUI uniform buffer is null.");

        if (TryBindTexture(texId, systemTime))
        {
            guiResourceManager.GuiUniformBuffer.Buffer[0] = vertexConstants;
            guiResourceManager.GuiUniformBuffer.Update(commandList);
        }

        // Execute draw call.
        commandList.DrawIndexed(curCmd.ElemCount, 1,
            (uint)(indexOffset + listIndexOffset), vertexOffset, 0);
    }

    /// <summary>
    ///     Binds the texture for a draw call.
    /// </summary>
    /// <param name="texId">GUI texture ID of next layer.</param>
    /// <param name="systemTime">Current system time.</param>
    /// <returns>
    ///     true if vertex shader constants were updated, false otherwise.
    /// </returns>
    private bool TryBindTexture(IntPtr texId, ulong systemTime)
    {
        if (texId == IntPtr.Zero || texId == lastTexture) return false;
        if (atlasManager.TextureAtlas == null) throw new InvalidOperationException("Texture atlas is null.");

        // Resolve the texture ID to an offset into the texture atlas.
        float startX, startY, endX, endY;
        if (texId == GuiFontAtlas.TextureId)
        {
            // Font render.
            (startX, startY, endX, endY) = atlasManager.TextureAtlas.FontAtlasBounds;
        }
        else
        {
            // Something else being rendered - what?
            var textureData = textureMapper.GetTextureDataForTextureId(texId);
            switch (textureData.SourceType)
            {
                case GuiTextureMapper.SourceType.AnimatedSprite:
                    BindAnimatedSprite(textureData.Id, systemTime, out startX, out startY, out endX, out endY);
                    break;

                case GuiTextureMapper.SourceType.Multiple:
                    // Nested layers are not supported, skip.
                    startX = 0.0f;
                    startY = 0.0f;
                    endX = 0.0f;
                    endY = 0.0f;
                    break;

                case GuiTextureMapper.SourceType.Spritesheet:
                case GuiTextureMapper.SourceType.Sprite:
                default:
                    startX = textureData.StartX;
                    startY = textureData.StartY;
                    endX = textureData.EndX;
                    endY = textureData.EndY;
                    break;
            }
        }

        // Update the vertex shader constants with the new offset.
        vertexConstants.TextureStart = new Vector2(startX, startY);
        vertexConstants.TextureEnd = new Vector2(endX, endY);
        lastTexture = texId;
        return true;
    }

    /// <summary>
    ///     Binds an animated sprite for a draw call.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="systemTime">System time for the frame being rendered.</param>
    /// <param name="startX">Top-left X coordinate.</param>
    /// <param name="startY">Top-left Y coordinate.</param>
    /// <param name="endX">Bottom-right X coordinate.</param>
    /// <param name="endY">Bottom-right Y coordinate.</param>
    private void BindAnimatedSprite(int animatedSpriteId, ulong systemTime, out float startX, out float startY,
        out float endX, out float endY)
    {
        // Resolve animation to the sprite for the current frame.
        if (animatedSpriteId >= animatedSpriteManager.AnimatedSprites.Count)
        {
            // Animated sprite was removed before rendering, blank the draw.
            startX = 0.0f;
            startY = 0.0f;
            endX = 0.0f;
            endY = 0.0f;
            return;
        }

        var animSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].Phases[AnimationPhase.Default];
        var sprite = animSprite.GetSpriteForTime(systemTime, Orientation.South);

        // Resolve sprite to texture atlas offset.
        var mapElem = atlasMap.MapElements[sprite.Id];
        startX = mapElem.NormalizedLeftX;
        startY = mapElem.NormalizedTopY;
        endX = mapElem.NormalizedRightX;
        endY = mapElem.NormalizedBottomY;
    }

    /// <summary>
    ///     Updates the GUI vertex and index byffers for the current frame.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    /// <param name="drawData">Dear ImGui draw data for current frame.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private unsafe void UpdateBuffers(CommandList commandList, ImDrawDataPtr drawData)
    {
        // Second stage, set up GUI rendering resources.
        // Check validity.
        var vertexBuf = guiResourceManager.GuiVertexBuffer;
        var indexBuf = guiResourceManager.GuiIndexBuffer;
        if (vertexBuf == null || indexBuf == null)
            throw new InvalidOperationException("Tried to render GUI without buffers.");
        if (drawData.TotalVtxCount > vertexBuf.Length)
            throw new InvalidOperationException("Vertex buffer too small for GUI render.");
        if (drawData.TotalIdxCount > indexBuf.Length)
            throw new InvalidOperationException("Index buffer too small for GUI render.");

        // Populate buffers.
        var vertexOffset = 0;
        var indexOffset = 0;
        var vertexSize = Marshal.SizeOf(typeof(ImDrawVert));
        var indexSize = Marshal.SizeOf<ushort>();
        for (var i = 0; i < drawData.CmdListsCount; ++i)
        {
            var curList = drawData.CmdLists[i];

            // Grab buffers from ImGui.
            var vertexIn = curList.VtxBuffer.Data.ToPointer();
            var vertexOut = (vertexBuf.BufferPtr + vertexOffset * vertexSize).ToPointer();
            var vertexBytes = curList.VtxBuffer.Size * vertexSize;

            var indexIn = curList.IdxBuffer.Data.ToPointer();
            var indexOut = (indexBuf.BufferPtr + indexOffset * indexSize).ToPointer();
            var indexBytes = curList.IdxBuffer.Size * indexSize;

            // Copy into the device buffers. This is a redundant copy, but we can optimize it away later
            // if it's a significant performance impact.
            Buffer.MemoryCopy(vertexIn, vertexOut,
                vertexSize * (vertexBuf.Length - vertexOffset),
                vertexBytes);
            Buffer.MemoryCopy(indexIn, indexOut,
                indexSize * (indexBuf.Length - indexOffset),
                indexBytes);

            vertexOffset += curList.VtxBuffer.Size;
            indexOffset += curList.IdxBuffer.Size;
        }

        // Synchronize and select buffers.
        vertexBuf.Update(commandList);
        indexBuf.Update(commandList);
        commandList.SetVertexBuffer(0, vertexBuf.DeviceBuffer);
        commandList.SetIndexBuffer(indexBuf.DeviceBuffer, IndexFormat.UInt16);
    }

    /// <summary>
    ///     Creates the resource set for GUI rendering.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if resources are not properly initialized.</exception>
    private void CreateResourceSet()
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (guiResourceManager.GuiUniformBuffer == null)
            throw new InvalidOperationException("Vertex uniform buffer not ready.");
        if (resourceManager.AtlasTexture == null)
            throw new InvalidOperationException("Texture atlas not ready.");

        var resLayoutDesc = new ResourceLayoutDescription(new ResourceLayoutElementDescription(
            GameResourceManager.RES_SHADER_CONSTANTS,
            ResourceKind.UniformBuffer,
            ShaderStages.Vertex
        ), new ResourceLayoutElementDescription(
            GameResourceManager.RES_TEXTURE_ATLAS,
            ResourceKind.TextureReadOnly,
            ShaderStages.Fragment
        ), new ResourceLayoutElementDescription(
            GameResourceManager.RES_TEXTURE_ATLAS_SAMPLER,
            ResourceKind.Sampler,
            ShaderStages.Fragment
        ));
        var resLayout = device.Device.ResourceFactory.CreateResourceLayout(resLayoutDesc);

        var resSetDesc = new ResourceSetDescription(
            resLayout,
            guiResourceManager.GuiUniformBuffer.DeviceBuffer,
            resourceManager.AtlasTexture.TextureView,
            device.Device.PointSampler
        );
        resourceSet = device.Device.ResourceFactory.CreateResourceSet(resSetDesc);
    }
}