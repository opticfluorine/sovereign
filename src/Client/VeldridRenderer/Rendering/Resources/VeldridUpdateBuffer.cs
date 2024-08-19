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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Manages an updateable Veldrid device buffer.
/// </summary>
public class VeldridUpdateBuffer<T> : IDisposable
    where T : unmanaged
{
    /// <summary>
    ///     Length of the local buffer in bytes.
    /// </summary>
    private readonly uint bufferLenBytes;

    /// <summary>
    ///     Whether the buffer supports multiple updates per frame.
    /// </summary>
    private readonly bool isMultiUpdate;

    /// <summary>
    ///     Buffers not yet used this frame.
    /// </summary>
    private readonly Stack<PinnedBuffer> unusedBuffers = new();

    /// <summary>
    ///     Buffers already used this frame.
    /// </summary>
    private readonly Stack<PinnedBuffer> usedBuffers = new();

    private PinnedBuffer currentBuffer;

    /// <summary>
    ///     Creates a new updateable buffer.
    /// </summary>
    /// <param name="device">Veldrid device.</param>
    /// <param name="usage">Buffer usage.</param>
    /// <param name="sizeInT">Number of T objects in the buffer.</param>
    /// <param name="isMultiUpdate">Whether the buffer should support multiple updates per frame.</param>
    public VeldridUpdateBuffer(VeldridDevice device, BufferUsage usage,
        uint sizeInT, bool isMultiUpdate = false)
    {
        this.isMultiUpdate = isMultiUpdate;
        Length = sizeInT;
        UsedLength = Length;
        ElementSize = (uint)Marshal.SizeOf<T>();
        bufferLenBytes = sizeInT * ElementSize;
        currentBuffer = new PinnedBuffer(sizeInT);
        CreateBuffers(device, usage);
    }

    /// <summary>
    ///     Pointer to the beginning of the local buffer.
    /// </summary>
    public IntPtr BufferPtr => currentBuffer.BufferPtr;

    /// <summary>
    ///     Backing Veldrid device buffer.
    /// </summary>
    public DeviceBuffer DeviceBuffer { get; private set; }

    /// <summary>
    ///     Number of T objects in the buffer.
    /// </summary>
    public uint Length { get; }

    /// <summary>
    ///     Local buffer for update.
    /// </summary>
    public T[] Buffer => currentBuffer.Buffer;

    /// <summary>
    ///     Size of each element in bytes.
    /// </summary>
    public uint ElementSize { get; }

    /// <summary>
    ///     Number of T objects currently set in the buffer.
    /// </summary>
    public uint UsedLength { get; set; }

    public void Dispose()
    {
        DeviceBuffer.Dispose();
        while (usedBuffers.TryPop(out var buf)) buf.Dispose();

        while (unusedBuffers.TryPop(out var buf)) buf.Dispose();

        currentBuffer.Dispose();
    }

    /// <summary>
    ///     Updates the buffer by copying the staging buffer to the GPU buffer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Update(CommandList commandList)
    {
        commandList.UpdateBuffer(DeviceBuffer, 0, currentBuffer.BufferPtr,
            Math.Min(bufferLenBytes, ElementSize * UsedLength));
        if (isMultiUpdate)
        {
            usedBuffers.Push(currentBuffer);
            if (unusedBuffers.TryPop(out var nextBuffer))
                currentBuffer = nextBuffer;
            else
                currentBuffer = new PinnedBuffer(Length);
        }
    }

    /// <summary>
    ///     Resets state for the end of a frame.
    /// </summary>
    public void EndFrame()
    {
        while (usedBuffers.TryPop(out var nextBuffer))
            unusedBuffers.Push(nextBuffer);
    }

    /// <summary>
    ///     Creates the buffer.
    /// </summary>
    /// <param name="device">Veldrid device.</param>
    /// <param name="usage">Intended usage of the buffer.</param>
    [MemberNotNull("DeviceBuffer")]
    private void CreateBuffers(VeldridDevice device, BufferUsage usage)
    {
        if (device.Device == null)
            throw new InvalidOperationException("Tried to create buffers without device.");
        var desc = new BufferDescription(bufferLenBytes, usage);
        DeviceBuffer = device.Device.ResourceFactory.CreateBuffer(desc);
    }

    /// <summary>
    ///     Struct containing a GC-pinned buffer.
    /// </summary>
    private class PinnedBuffer : IDisposable
    {
        /// <summary>
        ///     Buffer array.
        /// </summary>
        public readonly T[] Buffer;

        /// <summary>
        ///     Buffer pointer.
        /// </summary>
        public readonly IntPtr BufferPtr;

        /// <summary>
        ///     Garbage collector handle for pinning.
        /// </summary>
        public GCHandle GcHandle;

        public PinnedBuffer(uint length)
        {
            Buffer = new T[length];
            GcHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            BufferPtr = GcHandle.AddrOfPinnedObject();
        }

        public void Dispose()
        {
            if (GcHandle.IsAllocated) GcHandle.Free();
        }
    }
}