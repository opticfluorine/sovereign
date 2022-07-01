/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System.Runtime.InteropServices;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
/// Manages an updateable Veldrid device buffer.
/// </summary>
public class VeldridUpdateBuffer<T> : System.IDisposable
    where T : unmanaged
{

    /// <summary>
    /// Backing Veldrid device buffer.
    /// </summary>
    public DeviceBuffer DeviceBuffer { get; private set; }

    /// <summary>
    /// Number of T objects in the buffer.
    /// </summary>
    public uint Length { get; private set; }

    /// <summary>
    /// Local buffer for update.
    /// </summary>
    public T[] Buffer { get; private set; }

    /// <summary>
    /// Size of each element in bytes.
    /// </summary>
    public uint ElementSize { get; private set; }

    /// <summary>
    /// GC handle for the local buffer.
    /// </summary>
    private GCHandle bufferHandle;

    /// <summary>
    /// Pointer to the beginning of the local buffer.
    /// </summary>
    private System.IntPtr bufferPtr;

    /// <summary>
    /// Length of the local buffer in bytes.
    /// </summary>
    private uint bufferLenBytes;

    /// <summary>
    /// Creates a new updateable buffer.
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
    /// Updates the buffer by copying the staging buffer to the GPU buffer.
    /// </summary>
    /// <param name="commandList">Active command list.</param>
    public void Update(CommandList commandList)
    {
        commandList.UpdateBuffer(DeviceBuffer, 0, bufferPtr, bufferLenBytes);
    }

    public void Dispose()
    {
        DeviceBuffer?.Dispose();
        bufferHandle.Free();
    }

    /// <summary>
    /// Creates the buffer.
    /// </summary>
    /// <param name="device">Veldrid device.</param>
    private void CreateBuffers(VeldridDevice device, BufferUsage usage)
    {
        var desc = new BufferDescription(bufferLenBytes, usage);
        DeviceBuffer = device.Device.ResourceFactory.CreateBuffer(desc);
    }

    /// <summary>
    /// Allocates the local update buffer.
    /// </summary>
    private void AllocateLocalMemory()
    {
        Buffer = new T[Length];
        bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        bufferPtr = bufferHandle.AddrOfPinnedObject();
        bufferLenBytes = Length * ElementSize;
    }

}
