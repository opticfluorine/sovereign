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
    ///     GC handle for the local buffer.
    /// </summary>
    private GCHandle bufferHandle;

    /// <summary>
    ///     Length of the local buffer in bytes.
    /// </summary>
    private uint bufferLenBytes;

    /// <summary>
    ///     Pointer to the beginning of the local buffer.
    /// </summary>
    private IntPtr bufferPtr;

    /// <summary>
    ///     Creates a new updateable buffer.
    /// </summary>
    /// <param name="device">Veldrid device.</param>
    /// <param name="usage">Buffer usage.</param>
    /// <param name="sizeInT">Number of T objects in the buffer.</param>
    public VeldridUpdateBuffer(VeldridDevice device, BufferUsage usage,
        uint sizeInT)
    {
        Length = sizeInT;
        ElementSize = (uint)Marshal.SizeOf<T>();
        AllocateLocalMemory();
        CreateBuffers(device, usage);
    }

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
    public T[] Buffer { get; private set; }

    /// <summary>
    ///     Size of each element in bytes.
    /// </summary>
    public uint ElementSize { get; }

    public void Dispose()
    {
        DeviceBuffer?.Dispose();
        bufferHandle.Free();
    }

    /// <summary>
    ///     Updates the buffer by copying the staging buffer to the GPU buffer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Update(CommandList commandList)
    {
        commandList.UpdateBuffer(DeviceBuffer, 0, bufferPtr, bufferLenBytes);
    }

    /// <summary>
    ///     Creates the buffer.
    /// </summary>
    /// <param name="device">Veldrid device.</param>
    private void CreateBuffers(VeldridDevice device, BufferUsage usage)
    {
        var desc = new BufferDescription(bufferLenBytes, usage);
        DeviceBuffer = device.Device.ResourceFactory.CreateBuffer(desc);
    }

    /// <summary>
    ///     Allocates the local update buffer.
    /// </summary>
    private void AllocateLocalMemory()
    {
        Buffer = new T[Length];
        bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        bufferPtr = bufferHandle.AddrOfPinnedObject();
        bufferLenBytes = Length * ElementSize;
    }
}