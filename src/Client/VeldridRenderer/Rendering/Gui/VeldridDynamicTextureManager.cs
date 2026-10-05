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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sovereign.ClientCore.Rendering.Gui;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Deferred operation against a dynamic texture, queued from any thread and
///     applied on the render thread.
/// </summary>
/// <param name="Handle">Texture handle.</param>
/// <param name="Type">Operation type.</param>
/// <param name="Pixels">Pixel data for update operations, or null.</param>
/// <param name="RemovedTexture">
///     Removed texture instance for remove operations. The caller thread detaches the
///     instance from the live table, so the op must carry it for the render thread to
///     dispose the GPU resources.
/// </param>
internal readonly record struct DynamicTextureOp(
    int Handle,
    DynamicTextureOp.OpType Type,
    byte[]? Pixels,
    DynamicTexture? RemovedTexture)
{
    public enum OpType
    {
        Add,
        Update,
        Remove
    }
}

/// <summary>
///     Veldrid-backed implementation of <see cref="IDynamicTextureManager"/>.
/// </summary>
/// <remarks>
///     <para>
///         All public methods are thread-safe. Mutating operations are queued and applied
///         on the render thread by <see cref="ApplyPendingOps"/>, called at the start of
///         each frame by the GUI renderer.
///     </para>
///     <para>
///         Operations are applied in the order they were enqueued, so an update enqueued
///         after an add is guaranteed to be applied after the GPU texture exists. A remove
///         enqueued before the first apply cancels any pending add or update; a remove
///         after the GPU texture exists disposes it.
///     </para>
/// </remarks>
public sealed class VeldridDynamicTextureManager : IDynamicTextureManager
{
    /// <summary>
    ///     Live dynamic textures by handle. Guarded by its lock; an entry is removed
    ///     as soon as RemoveTexture is called, before the GPU resources are released.
    /// </summary>
    private readonly Dictionary<int, DynamicTexture> textures = new();

    /// <summary>
    ///     Pending operations from any thread. Drained on the render thread.
    /// </summary>
    private readonly ConcurrentQueue<DynamicTextureOp> pendingOps = new();

    private readonly VeldridDevice device;

    private readonly GuiResourceManager guiResourceManager;

    private readonly GuiPipeline guiPipeline;

    private readonly GuiTextureMapper textureMapper;

    private int nextHandle;

    /// <summary>
    ///     Event raised on the render thread when a texture is removed.
    ///     The parameter is the handle of the removed texture.
    /// </summary>
    public event Action<int>? TextureRemoved;

    public VeldridDynamicTextureManager(VeldridDevice device, GuiTextureMapper textureMapper,
        GuiResourceManager guiResourceManager, GuiPipeline guiPipeline)
    {
        this.device = device;
        this.textureMapper = textureMapper;
        this.guiResourceManager = guiResourceManager;
        this.guiPipeline = guiPipeline;
    }

    public int AddTexture(uint width, uint height, DynamicTextureSampling sampling)
    {
        if (width == 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height == 0) throw new ArgumentOutOfRangeException(nameof(height));

        var handle = Interlocked.Increment(ref nextHandle);
        var dynTex = new DynamicTexture(device, width, height, sampling);
        lock (textures)
        {
            textures[handle] = dynTex;
        }

        pendingOps.Enqueue(new DynamicTextureOp(handle, DynamicTextureOp.OpType.Add, null, null));
        return handle;
    }

    public void UpdateTexture(int handle, ReadOnlySpan<byte> rgbaPixels)
    {
        lock (textures)
        {
            if (!textures.TryGetValue(handle, out var dynTex))
                throw new ArgumentException($"Unknown dynamic texture handle {handle}.", nameof(handle));
            var expected = checked((int)(dynTex.Width * dynTex.Height * 4));
            if (rgbaPixels.Length != expected)
                throw new ArgumentException(
                    $"Pixel buffer must be exactly {expected} bytes for texture {handle}, got {rgbaPixels.Length}.",
                    nameof(rgbaPixels));
        }

        // Copy out of the caller's span so it can be released immediately.
        var pixels = rgbaPixels.ToArray();

        pendingOps.Enqueue(new DynamicTextureOp(handle, DynamicTextureOp.OpType.Update, pixels, null));
    }

    public void RemoveTexture(int handle)
    {
        DynamicTexture? dynTex;
        lock (textures)
        {
            if (!textures.Remove(handle, out dynTex)) return;
        }

        // Carry the instance in the op: the live table no longer holds it.
        pendingOps.Enqueue(new DynamicTextureOp(handle, DynamicTextureOp.OpType.Remove, null, dynTex));
    }

    public bool TryGetDimensions(int handle, out uint width, out uint height)
    {
        lock (textures)
        {
            if (textures.TryGetValue(handle, out var dynTex))
            {
                width = dynTex.Width;
                height = dynTex.Height;
                return true;
            }
        }

        width = 0;
        height = 0;
        return false;
    }

    /// <summary>
    ///     Applies all pending operations on the render thread. Called once per frame
    ///     by the GUI renderer before draw commands are processed.
    /// </summary>
    public void ApplyPendingOps()
    {
        while (pendingOps.TryDequeue(out var op))
        {
            switch (op.Type)
            {
                case DynamicTextureOp.OpType.Add:
                    ApplyAdd(op.Handle);
                    break;

                case DynamicTextureOp.OpType.Update:
                    ApplyUpdate(op.Handle, op.Pixels);
                    break;

                case DynamicTextureOp.OpType.Remove:
                    ApplyRemove(op.Handle, op.RemovedTexture);
                    break;
            }
        }
    }

    /// <summary>
    ///     Gets the live texture for a handle, or null if unknown. Render thread use.
    /// </summary>
    internal DynamicTexture? GetTexture(int handle)
    {
        lock (textures)
        {
            return textures.TryGetValue(handle, out var dynTex) ? dynTex : null;
        }
    }

    private void ApplyAdd(int handle)
    {
        var dynTex = GetTexture(handle);
        if (dynTex == null) return; // removed before creation
        dynTex.CreateGpuTexture();
        CreateResourceSet(dynTex);
    }

    private void ApplyUpdate(int handle, byte[]? pixels)
    {
        var dynTex = GetTexture(handle);
        if (dynTex == null || pixels == null) return; // removed before update applied
        if (dynTex.Texture == null) return; // add op not yet applied; update follows it anyway

        dynTex.UploadPixels(pixels);
    }

    private void ApplyRemove(int handle, DynamicTexture? dynTex)
    {
        if (dynTex == null) return;
        dynTex.Dispose();
        TextureRemoved?.Invoke(handle);
    }

    /// <summary>
    ///     Creates the GUI resource set for a dynamic texture.
    /// </summary>
    private void CreateResourceSet(DynamicTexture dynTex)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Device not ready.");
        if (guiResourceManager.GuiUniformBuffer == null)
            throw new InvalidOperationException("GUI uniform buffer not ready.");
        if (guiPipeline.ResourceLayout == null)
            throw new InvalidOperationException("GUI pipeline not initialized.");

        var sampler = dynTex.Sampling switch
        {
            DynamicTextureSampling.Point => device.Device.PointSampler,
            DynamicTextureSampling.Linear => device.Device.LinearSampler,
            _ => throw new ArgumentOutOfRangeException(nameof(dynTex.Sampling))
        };

        var desc = new ResourceSetDescription(guiPipeline.ResourceLayout,
            guiResourceManager.GuiUniformBuffer.DeviceBuffer,
            dynTex.Texture!.TextureView,
            sampler);
        dynTex.ResourceSet = device.Device.ResourceFactory.CreateResourceSet(desc);
    }
}
